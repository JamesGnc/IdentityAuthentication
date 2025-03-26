using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.DTO;
using IdentityAuthentication_Master.Models.Tables;
using IdentityAuthentication_Master.Models.VO;
using IdentityAuthentication_Master.Servies.IndentityService;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.ComponentModel;

namespace IdentityAuthentication_Master.Controllers.IndentityIdName
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class IdentityController : ControllerBase
    {

        private readonly SqlSugarClient db;
        private readonly IdentityUserInfoServiceImpl _identityUserInfo;
        private readonly IdentityUserInfoMockServiceImpl _identityUserInfoMock;

        public IdentityController(IdentityUserInfoServiceImpl identityUserInfo, IdentityUserInfoMockServiceImpl identityUserInfoMock, SqlSugarClient sqlSugarClient)
        {
            _identityUserInfo = identityUserInfo;
            _identityUserInfoMock = identityUserInfoMock;
            db = sqlSugarClient;
        }

        [HttpPost]
        [Description("实名认证接口")]
        public ResponseResult<IdentityVerificationResultDTO> VerifyIdentity([FromBody] UserDataInfoVO param)
        {
            if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
            {
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, "参数错误");
            }

            // 开始调用接口
            try
            {
                // 真实查询腾讯云 Tencent Cloud API 接口
                //var result = await _identityUserInfo.VerifyIdentityAsync(param.Name, param.IdCard);
                //return ResponseResult<IdentityVerificationResultDTO>.Success(result);

                var dbResult = db.Queryable<UserIdentityInfos>().Where(it => it.IdCard == param.IdCard).ToList().FirstOrDefault();

                if (dbResult != null && dbResult.Name == param.Name)
                {
                    // 数据已存在
                    return ResponseResult<IdentityVerificationResultDTO>.Success(null!, "数据一致");
                }

                // 模拟查询腾讯云 成功和失败是随机的（信息是否一致）
                var result = _identityUserInfoMock.VerifyIdentity(param.Name, param.IdCard);

                if (result.ResultCode == "0")
                {
                    // 一致，那么就把数据存入数据库 方便后续查询
                    var insertObj = new UserIdentityInfos
                    {
                        Name = param.Name,
                        IdCard = param.IdCard,
                        CreateTime = DateTime.Now
                    };
                    var res = db.Insertable(insertObj).ExecuteReturnIdentity();
                    if(res > 0)
                    {
                        return ResponseResult<IdentityVerificationResultDTO>.Success(result, "数据一致，并且数据插入成功");
                    }
                    else
                    {
                        return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, "数据插入失败");
                    }
                }
                else
                {

                    // 不一致 那么就返回错误信息

                    return ResponseResult<IdentityVerificationResultDTO>.Failure(result, "身份信息不一致");
                }


            }
            catch (HttpRequestException ex)
            {
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, $"API请求失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, $"系统错误: {ex.Message}");
            }

        }


        [HttpPost]
        [Description("批量实名认证接口")]
        public ResponseResult<object> VerifyIdentityBatch([FromBody] List<UserDataInfoVO> paramsList)
        {
            if (paramsList == null || paramsList.Count == 0)
            {
                return ResponseResult<object>.Failure(null!, "参数错误");
            }

            // 提取所有的身份证号
            var idCardList = paramsList.Select(x => x.IdCard).ToList();

            // 1️⃣ 批量查询数据库，减少 SQL 查询次数
            var dbResults = db.Queryable<UserIdentityInfos>()
                              .Where(it => idCardList.Contains(it.IdCard))
                              .ToList()
                              .ToDictionary(it => it.IdCard, it => it.Name);

            List<UserDataInfoVO> inconsistentList = new List<UserDataInfoVO>();  // 记录不一致的
            List<UserDataInfoVO> toVerifyList = new List<UserDataInfoVO>();      // 需要腾讯云认证的

            foreach (var param in paramsList)
            {
                if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
                {
                    inconsistentList.Add(param);
                    continue;
                }

                if (dbResults.TryGetValue(param.IdCard, out string? existingName))
                {
                    if (existingName != param.Name)
                    {
                        inconsistentList.Add(param); // 已存在但名字不匹配
                    }
                }
                else
                {
                    toVerifyList.Add(param); // 需要查询腾讯云 API
                }
            }

            // 2️⃣ 如果所有数据都在数据库中且一致，直接返回
            if (inconsistentList.Count == 0 && toVerifyList.Count == 0)
            {
                return ResponseResult<object>.Success(null!, "全部一致");
            }

            // 3️⃣ 需要调用腾讯云 API 进行验证的部分
            List<UserDataInfoVO> verifiedInconsistentList = new List<UserDataInfoVO>(); // 记录腾讯云返回不一致的
            List<UserIdentityInfos> newInsertList = new List<UserIdentityInfos>(); // 记录需要插入数据库的数据

            foreach (var param in toVerifyList)
            {
                var result = _identityUserInfoMock.VerifyIdentity(param.Name!, param.IdCard!);
                if (result.ResultCode == "0")
                {
                    newInsertList.Add(new UserIdentityInfos
                    {
                        Name = param.Name!,
                        IdCard = param.IdCard!,
                        CreateTime = DateTime.Now
                    });
                }
                else
                {
                    verifiedInconsistentList.Add(param); // 腾讯云返回不一致的
                }
            }

            // 4️⃣ 批量插入数据库，避免循环插入
            if (newInsertList.Count > 0)
            {
                db.Insertable(newInsertList).ExecuteCommand();
            }

            // 5️⃣ 计算最终不一致的列表
            inconsistentList.AddRange(verifiedInconsistentList);

            if (inconsistentList.Count > 0)
            {
                return ResponseResult<object>.Failure(inconsistentList, "以下身份信息不一致");
            }

            return ResponseResult<object>.Success(null!, "全部一致，并插入成功");
        }


    }
}
