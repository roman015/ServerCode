using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyLayer.Middlewares
{
    public static class RequestFilterLayerExtensions
    {
        public static IApplicationBuilder UseRequestFilter(this IApplicationBuilder builder, IConfiguration configuration)
        {
            return builder.UseMiddleware<RequestFilterLayer>(configuration);
        }
    }

    public class RequestFilterLayer
    {
        private readonly RequestDelegate Next;
        private readonly IConfiguration Configuration;

        private Boolean BypassRequestFilter = false;

        private Boolean AllowEmptyUserAgent = false;
        private List<String> AllowedAgents;
        private List<String> BlockedAgents;

        public RequestFilterLayer(RequestDelegate Next, IConfiguration configuration)
        {
            this.Next = Next;
            this.Configuration = configuration;

            AllowedAgents = new List<string>();
            BlockedAgents = new List<string>();

            AllowEmptyUserAgent = Convert.ToBoolean(Configuration["AllowEmptyUserAgent"]);
            Console.WriteLine("AllowEmptyUserAgent: " + AllowEmptyUserAgent);
            BypassRequestFilter = Convert.ToBoolean(Configuration["BypassRequestFilter"]);
            Console.WriteLine("BypassRequestFilter: " + BypassRequestFilter);

            // Get List of allowed user agents
            while (!String.IsNullOrEmpty(Configuration["UserAgents:Allowed:" + AllowedAgents.Count]))
            {
                AllowedAgents.Add(Configuration["UserAgents:Allowed:" + AllowedAgents.Count]);
                Console.WriteLine("Allowed: " + AllowedAgents.Last());
            }

            // Get List of blocked user agents
            while (!String.IsNullOrEmpty(Configuration["UserAgents:Blocked:" + BlockedAgents.Count]))
            {
                BlockedAgents.Add(Configuration["UserAgents:Blocked:" + BlockedAgents.Count]);
                Console.WriteLine("Blocked: " + BlockedAgents.Last());
            }

        }

        public async Task Invoke(HttpContext context)
        {
            string UserAgent = context.Request.Headers["User-Agent"].ToString();

            // If Bypass flag is set, then let any request get processed
            if (BypassRequestFilter)
            {
                await Next.Invoke(context);
                return;
            }

            // If there is no user agent string, then block the request with a 500 error
            if (String.IsNullOrEmpty(UserAgent) && AllowEmptyUserAgent)
            {
                Console.WriteLine("Empty User Agent, ignoring request");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            // If user agent matches blocked user agents, return 500
            if (IsBlockedUserAgent(UserAgent))
            {
                Console.WriteLine("Blocked User Agent, ignoring request");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            // If user agent is not in whitelisted user agents, Log it
            if (!IsAllowedUserAgent(UserAgent))
            {
                Console.WriteLine("Unknown User Agent, allowing request for now");
            }

            await Next.Invoke(context);
        }

        private bool IsBlockedUserAgent(string UserAgent)
        {
            foreach (string agent in UserAgent.Split(' '))
            {
                if (!agent.StartsWith("(") && BlockedAgents.Contains(agent.Split('/')[0]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAllowedUserAgent(string UserAgent)
        {
            foreach (string agent in UserAgent.Split(' '))
            {
                if (!agent.StartsWith("(") && AllowedAgents.Contains(agent.Split('/')[0]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
