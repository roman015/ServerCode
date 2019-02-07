using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeployManager.Controllers
{
    [Route("Build")]
    [ApiController]
    public class BuildController : ControllerBase
    {
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok("Message From " + HttpContext.Request.Host.Host);
        }

        [HttpPost]
        public ActionResult ProcessWebHook()
        {
            try
            {
                Console.WriteLine("Message From " + HttpContext.Request.Host.Host);

                // Process only valid requests
                if (!HttpContext.Request.Headers.Keys.Contains("X-GitHub-Event"))
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                string requestBodyJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();

                // Process Github Request
                switch (HttpContext.Request.Headers["X-GitHub-Event"])
                {
                    case "ping":
                        PingPayload pingPayload = JsonConvert.DeserializeObject<PingPayload>(requestBodyJson);
                        return OnPingHandler(pingPayload);
                    case "push":
                        PushPayload pushPayload = JsonConvert.DeserializeObject<PushPayload>(requestBodyJson);
                        return OnPushHandler(pushPayload);
                    default:
                        Console.WriteLine("Invalid github event"
                            + HttpContext.Request.Headers["X-GitHub-Event"]);
                        return StatusCode(StatusCodes.Status501NotImplemented);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Ping to check if configured properly        
        public ActionResult OnPingHandler(PingPayload payload)
        {
            if(payload == null)
            {
                Console.WriteLine(DateTime.Today.ToString("MM/dd/yy H:mm:ss zzz") + " : No Payload Received");

                return StatusCode(StatusCodes.Status204NoContent);
            }

            Console.WriteLine(DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz") + " : Payload received {" + Environment.NewLine +
                "zen : " + payload.zen + Environment.NewLine +
                "hook_id" + payload.hook_id + Environment.NewLine +
                "repository.full_name" + payload.repository.full_name + Environment.NewLine +
                "}");

            return Ok();
        }

        // Handle Push Request     
        public ActionResult OnPushHandler([FromBody]PushPayload payload)
        {
            Console.WriteLine("New Push Detected");
            return Ok();
        }        
    }
}
