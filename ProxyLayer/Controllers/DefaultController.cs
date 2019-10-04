using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProxyLayer.Controllers
{
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet("robots.txt")]
        public IActionResult GetRobotsSettings()
        {
            // TODO : Read this from a text file
            return Ok("User-agent: *\r\nDisallow: /");
        }        
    }
}
