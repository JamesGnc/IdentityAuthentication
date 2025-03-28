using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.DTO;
using IdentityAuthentication_Master.Models.Tables;
using IdentityAuthentication_Master.Models.VO;
using IdentityAuthentication_Master.Servies.IndentityService;
using IdentityAuthentication_Master.Utiles;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityAuthentication_Master.Controllers.IndentityIdName
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class IdentityMockController : ControllerBase
    {
        private readonly SqlSugarClient db;
        private readonly IdentityUserInfoServiceImpl _identityUserInfo;
        private readonly IdentityUserInfoMockServiceImpl _identityUserInfoMock;

        public IdentityMockController(IdentityUserInfoServiceImpl identityUserInfo, IdentityUserInfoMockServiceImpl identityUserInfoMock, SqlSugarClient sqlSugarClient)
        {
            _identityUserInfo = identityUserInfo;
            _identityUserInfoMock = identityUserInfoMock;
            db = sqlSugarClient;
        }

        [HttpPost]
        [Description("实名认证接口")]
        public async Task<ResponseResult<IdentityVerificationResultDTO>> VerifyIdentity([FromBody] UserDataInfoVO param)
        {
            if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
            {
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, "参数错误");
            }

            try
            {
                var dbResult = await GetUserIdentityInfoAsync(param.IdCard);

                if (dbResult != null && dbResult.Name == param.Name)
                {
                    return ResponseResult<IdentityVerificationResultDTO>.Success(null!, "数据一致");
                }

                // 真实腾讯云
                //var result = await _identityUserInfo.VerifyIdentityAsync(param.Name, param.IdCard);

                var result = _identityUserInfoMock.VerifyIdentity(param.Name, param.IdCard);
                string encryptedIdCard = AesEncryptionHelper.Encrypt(param.IdCard);

                if (result.ResultCode == "0")
                {
                    var insertObj = new UserIdentityInfos
                    {
                        Name = param.Name,
                        IdCard = encryptedIdCard,
                        CreateTime = DateTime.Now
                    };
                    var res = await db.Insertable(insertObj).ExecuteReturnIdentityAsync();
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
                    return ResponseResult<IdentityVerificationResultDTO>.Failure(result, "身份信息不一致");
                }
            }
            catch (HttpRequestException ex)
            {
                // 添加日志记录
                // _logger.LogError(ex, "API请求失败");
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, $"API请求失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 添加日志记录
                // _logger.LogError(ex, "系统错误");
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!, $"系统错误: {ex.Message}");
            }
        }

        [HttpPost]
        [Description("批量实名认证接口")]
        public async Task<ResponseResult<object>> VerifyIdentityBatch([FromBody] List<UserDataInfoVO> paramsList)
        {
            if (paramsList == null || paramsList.Count == 0)
            {
                return ResponseResult<object>.Failure(null!, "参数错误");
            }

            List<UserDataInfoVO> inconsistentList = new List<UserDataInfoVO>();
            List<UserDataInfoVO> toVerifyList = new List<UserDataInfoVO>();

            var dbResultList = await db.Queryable<UserIdentityInfos>()
                                       .Where(it => it.IdCard != null)
                                       .ToListAsync();

            foreach (var param in paramsList)
            {
                if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
                {
                    inconsistentList.Add(param);
                    continue;
                }

                var dbResult = dbResultList.Where(it => AesEncryptionHelper.Decrypt(it.IdCard) == param.IdCard).FirstOrDefault();

                if (dbResult != null && dbResult.Name == param.Name)
                {
                    continue;
                }
                else
                {
                    toVerifyList.Add(param);
                }
            }

            if (inconsistentList.Count == 0 && toVerifyList.Count == 0)
            {
                return ResponseResult<object>.Success(null!, "全部一致");
            }

            List<UserIdentityInfos> newInsertList = new List<UserIdentityInfos>();
            List<UserDataInfoVO> verifiedInconsistentList = new List<UserDataInfoVO>();

            foreach (var param in toVerifyList)
            {
                var result = _identityUserInfoMock.VerifyIdentity(param.Name!, param.IdCard!);
                string encryptedIdCard = AesEncryptionHelper.Encrypt(param.IdCard!);

                if (result.ResultCode == "0")
                {
                    newInsertList.Add(new UserIdentityInfos
                    {
                        Name = param.Name!,
                        IdCard = encryptedIdCard,
                        CreateTime = DateTime.Now
                    });
                }
                else
                {
                    verifiedInconsistentList.Add(param);
                }
            }

            if (newInsertList.Count > 0)
            {
                await db.Insertable(newInsertList).ExecuteCommandAsync();
            }

            inconsistentList.AddRange(verifiedInconsistentList);

            if (inconsistentList.Count > 0)
            {
                return ResponseResult<object>.Failure(inconsistentList, "以下身份信息不一致");
            }

            return ResponseResult<object>.Success(null!, "全部一致，并插入成功");
        }

        private async Task<UserIdentityInfos?> GetUserIdentityInfoAsync(string idCard)
        {
            var dbResultList = await db.Queryable<UserIdentityInfos>()
                                       .Where(it => it.IdCard != null)
                                       .ToListAsync();

            return dbResultList.Where(it => AesEncryptionHelper.Decrypt(it.IdCard) == idCard).FirstOrDefault();
        }
    }
}
