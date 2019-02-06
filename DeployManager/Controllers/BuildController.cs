using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeployManager.Controllers
{
    [Route("build")]
    [ApiController]
    public class BuildController : ControllerBase
    {
        // Ping to check if configured properly
        [HttpPost("notify/ping")]
        public ActionResult OnPingHandler([FromBody]PingPayload payload)
        {
            if(payload == null)
            {
                Console.WriteLine(DateTime.Today.ToString("MM/dd/yy H:mm:ss zzz") + " : No Payload Received");

                return StatusCode(StatusCodes.Status204NoContent);
            }

            Console.WriteLine(DateTime.Today.ToString("MM/dd/yy H:mm:ss zzz") + " : Payload received {" +
                "zen : " + payload.zen +
                "hook_id" + payload.hook_id +
                "hook" + payload.hook +
                "}");

            return Ok();
        }

        // GET api/values
        [HttpPost("notify/push")]
        public ActionResult OnPushHandler([FromBody]PushPayload payload)
        {
            Console.WriteLine("New Push Detected");
            return Ok();
        }        
    }
}
