using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.DTO;
using IdentityAuthentication_Master.Models.VO;
using IdentityAuthentication_Master.Servies.IndentityService;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace IdentityAuthentication_Master.Controllers.IndentityIdName
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {

        private readonly IdentityUserInfoService _identityUserInfo;

        public IdentityController(IdentityUserInfoService identityUserInfo)
        {
            _identityUserInfo = identityUserInfo;
        }

        [HttpPost(Name = "VerifyIdentity")]
        [Description("实名认证接口")]
        public async Task<ResponseResult<IdentityVerificationResultDTO>> VerifyIdentity([FromBody] UserDataInfoVO param)
        {
            if (string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.IdCard))
            {
                return ResponseResult<IdentityVerificationResultDTO>.Failure(null!,"参数错误");
            }

            try
            {
                var result = await _identityUserInfo.VerifyIdentityAsync(param.Name, param.IdCard);
                return ResponseResult<IdentityVerificationResultDTO>.Success(result);
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
    }
}
