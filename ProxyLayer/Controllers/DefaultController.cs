using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ProxyLayer.Controllers
{
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly IConfiguration ACMEConfiguration;
        public DefaultController(IConfiguration configuration)
        {
            Console.WriteLine("Opening ACME File in : " + configuration["AcmeSettingsLocation"].ToString());

            if (configuration != null && configuration["AcmeSettingsLocation"] != null)
            {
                this.ACMEConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(path: configuration["AcmeSettingsLocation"].ToString(), optional: true, reloadOnChange: true)
                    .Build();
            }
        }

        [HttpGet("test")]
        public IActionResult TestServer()
        {            
            return Ok("It Works!");
        }

        [HttpGet("robots.txt")]
        public IActionResult GetRobotsSettings()
        {
            // TODO : Read this from a text file
            return Ok("User-agent: *\r\nDisallow: /");
        }

        [HttpGet(".well-known/acme-challenge/{ChallengeID}")]
        public IActionResult RespondToAcmeHttpChallenge(string ChallengeID)
        {
            // Respond only if Configured
            if (!Convert.ToBoolean(ACMEConfiguration["AcmeHttpResponse:Enabled"]))
            {
                Console.WriteLine("ERROR : ACME Disabled");
                return StatusCode(500);
            }

            // Return the response string only if ChallengeID is a match
            if (GetResponse(ChallengeID) != null)
            {
                Console.WriteLine("INFO : Valid Path '" + ChallengeID + "', Sending Response");

                Response.ContentType = "application/octet-stream";

                return Ok(GetResponse(ChallengeID));
            }
            else
            {
                Console.WriteLine("ERROR : Invalid Path '" + ChallengeID + "'");
                return StatusCode(500);
            }
        }

        private String GetResponse(String ChallengeID)
        {
            int count = int.Parse(ACMEConfiguration["AcmeHttpResponse:Count"]);

            for (int i = 0; i < count; i++)
            {
                if (ChallengeID.Equals(ACMEConfiguration["AcmeHttpResponse:Challenges:" + i.ToString() + ":Path"]))
                {
                    return ACMEConfiguration["AcmeHttpResponse:Challenges:" + i.ToString() + ":Value"];
                }
            }

            return null;
        }
    }
}
