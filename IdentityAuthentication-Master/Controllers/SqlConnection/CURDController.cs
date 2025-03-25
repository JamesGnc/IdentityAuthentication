using IdentityAuthentication_Master.Models;
using IdentityAuthentication_Master.Models.Tables;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;

namespace IdentityAuthentication_Master.Controllers.SqlConnection
{
    [ApiController]
    [Route("[controller]")]
    public class CURDController : ControllerBase
    {
        private readonly SqlSugarClient _db;

        public CURDController(SqlSugarClient db)
        {
            _db = db;
        }

        [HttpPost(Name = "GetAll")]
        [Description("获取全部信息")]
        public IActionResult GetCarDataInfoInfo(int? id)
        {
            var List = _db.Queryable<User>().Where(o => o.Id == id).ToList();
            return Ok(List);
        }
    }
}
