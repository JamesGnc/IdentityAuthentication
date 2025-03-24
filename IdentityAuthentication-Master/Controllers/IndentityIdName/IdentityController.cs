using IdentityAuthentication_Master.Models.VO;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.ComponentModel;

namespace IdentityAuthentication_Master.Controllers.IndentityIdName
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController
    {
        [ApiController]
        [Description("IndentityID")]
        [Route("api/[controller]/[action]")]
        public class GeraldController : ControllerBase
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly SqlSugarClient _db;

            public GeraldController(IHttpClientFactory httpClientFactory, SqlSugarClient db)
            {
                _httpClientFactory = httpClientFactory;
                _db = db;
            }

            [HttpGet(Name = "GetDate")]
            [Description("获取数据")]
            public IActionResult GetGeraldData([FromBody] UserDataInfoVO param)
            {
               
                //var successResult = ResponseResult<GeraldExp>.Success(item);
                return Ok();
            }

            //[HttpGet(Name = "GetList")]
            //[Description("获取数据列表")]
            //public IActionResult GetGeraldDataList([FromQuery] GeraldExpQueryDto param)
            //{
            //    var query = _db.Queryable<GeraldExp>();
            //    if (!string.IsNullOrEmpty(param.Name))
            //    {
            //        query = query.Where(o => o.Name.Contains(param.Name));
            //    }
            //    if (!string.IsNullOrEmpty(param.Description))
            //    {
            //        query = query.Where(o => o.Description!.Contains(param.Description));
            //    }
            //    if (!string.IsNullOrEmpty(param.Image))
            //    {
            //        query = query.Where(o => o.Image!.Contains(param.Image));
            //    }
            //    if (param.Status != null)
            //    {
            //        query = query.Where(o => o.Status == param.Status);
            //    }

            //    int total = 0;
            //    var list = query.ToPageList(param.PageIndex, param.PageSize, ref total);

            //    var result = ResponseResult<List<GeraldExp>>.SuccessList(list, total);
            //    return Ok(result);
            //}

            //[HttpPost(Name = "Create")]
            //[Description("创建数据")]
            //public IActionResult Create([FromBody] GeraldExpCreateDto param)
            //{
            //    var data = param.Adapt<GeraldExp>();
            //    var Id = _db.Insertable(data).ExecuteReturnIdentity();
            //    var result = ResponseResult<int>.Success(Id);
            //    return Ok(result);
            //}
        }
    }
}
