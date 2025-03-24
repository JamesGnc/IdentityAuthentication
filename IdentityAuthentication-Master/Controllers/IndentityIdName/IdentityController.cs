using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.DTO;
using IdentityAuthentication_Master.Models.VO;
using IdentityAuthentication_Master.Servies.IndentityService;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Mapster;
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

        [HttpGet(Name = "CheckInfo")]
        [Description("核实数据")]
        public ResponseResult GetGeraldData([FromQuery] UserDataInfoVO param)
        {
            if (param.UserName == null || param.UserIdNum == null)
            {
                return ResponseResult.ParamInvalid();
            }

            var dto = param.Adapt<IndentityUserInfoParamDTO>();
            var Resutlt = _identityUserInfo.IndentityUserInfo(dto);
            if (Resutlt == null) { return ResponseResult.BadRequest("Request Fail"); }
            return ResponseResult.Success();
        }

    }
}

