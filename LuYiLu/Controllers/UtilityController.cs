using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuYiLu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        [HttpGet("GB2312Encode")]
        public string GB2312Encode(string p)
        {
            return System.Web.HttpUtility.UrlEncode(p, Encoding.GetEncoding("gb2312"));
        }
    }
}