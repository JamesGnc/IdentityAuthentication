using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.DTO;
using IdentityAuthentication_Master.Models.Tables;
using IdentityAuthentication_Master.Models.VO;
using IdentityAuthentication_Master.Servies.IndentityService;
using IdentityAuthentication_Master.Utiles;
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

                //var dbResult = db.Queryable<UserIdentityInfos>().Where(it => it.IdCard == param.IdCard).ToList().FirstOrDefault();

                var dbResultList = db.Queryable<UserIdentityInfos>()
                                     .Where(it => it.IdCard != null)
                                     .ToList();

                var dbResult = dbResultList.Where(it => AesEncryptionHelper.Decrypt(it.IdCard) == param.IdCard).ToList().FirstOrDefault();

                if (dbResult != null && dbResult.Name == param.Name)
                {
                    // 数据已存在
                    return ResponseResult<IdentityVerificationResultDTO>.Success(null!, "数据一致");
                }

                // 模拟查询腾讯云 成功和失败是随机的（信息是否一致）
                var result = _identityUserInfoMock.VerifyIdentity(param.Name, param.IdCard);
                string encryptedIdCard = AesEncryptionHelper.Encrypt(param.IdCard);

                if (result.ResultCode == "0")
                {
                    // 一致，那么就把数据存入数据库 方便后续查询
                    var insertObj = new UserIdentityInfos
                    {
                        Name = param.Name,
                        IdCard = encryptedIdCard, // 加密存储
                        CreateTime = DateTime.Now
                    };
                    var res = db.Insertable(insertObj).ExecuteReturnIdentity();
                    if (res > 0)
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
                    // 不一致 返回错误信息
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

            List<UserDataInfoVO> inconsistentList = new List<UserDataInfoVO>();  // 记录不一致的
            List<UserDataInfoVO> toVerifyList = new List<UserDataInfoVO>();      // 需要腾讯云认证的

            // 1️⃣ 先查询数据库中所有已存储的加密身份证号
            var dbResultList = db.Queryable<UserIdentityInfos>()
                                 .Where(it => it.IdCard != null)
                                 .ToList();

            foreach (var param in paramsList)
            {
                if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
                {
                    inconsistentList.Add(param);
                    continue;
                }

                // 2️⃣ 遍历数据库数据，解密身份证号进行匹配
                var dbResult = dbResultList.Where(it => AesEncryptionHelper.Decrypt(it.IdCard) == param.IdCard).ToList().FirstOrDefault();

                if (dbResult != null && dbResult.Name == param.Name)
                {
                    // 数据一致，继续
                    continue;
                }
                else
                {
                    // 数据不存在或者姓名不一致，需要腾讯云认证
                    toVerifyList.Add(param);
                }
            }

            // 3️⃣ 如果所有数据都在数据库中且一致，直接返回
            if (inconsistentList.Count == 0 && toVerifyList.Count == 0)
            {
                return ResponseResult<object>.Success(null!, "全部一致");
            }

            // 4️⃣ 批量调用腾讯云 API 认证
            List<UserIdentityInfos> newInsertList = new List<UserIdentityInfos>(); // 需要插入数据库的数据
            List<UserDataInfoVO> verifiedInconsistentList = new List<UserDataInfoVO>(); // 记录腾讯云返回不一致的

            foreach (var param in toVerifyList)
            {
                var result = _identityUserInfoMock.VerifyIdentity(param.Name!, param.IdCard!);
                string encryptedIdCard = AesEncryptionHelper.Encrypt(param.IdCard!);

                if (result.ResultCode == "0")
                {
                    // 认证通过，存入数据库
                    newInsertList.Add(new UserIdentityInfos
                    {
                        Name = param.Name!,
                        IdCard = encryptedIdCard, // 加密存储
                        CreateTime = DateTime.Now
                    });
                }
                else
                {
                    // 认证失败
                    verifiedInconsistentList.Add(param);
                }
            }

            // 5️⃣ 批量插入数据库
            if (newInsertList.Count > 0)
            {
                db.Insertable(newInsertList).ExecuteCommand();
            }

            // 6️⃣ 计算最终不一致的列表
            inconsistentList.AddRange(verifiedInconsistentList);

            if (inconsistentList.Count > 0)
            {
                return ResponseResult<object>.Failure(inconsistentList, "以下身份信息不一致");
            }

            return ResponseResult<object>.Success(null!, "全部一致，并插入成功");
        }


    }
}
