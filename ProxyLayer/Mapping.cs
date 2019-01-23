using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyLayer
{
    public class Mapping
    {
        public string Pattern { get; set; }
        public int Port { get; set; }

        public Mapping(string pattern, int port)
        {
            Pattern = pattern;
            Port = port;
        }

        public ProxyOptions ProxyOptions => new ProxyOptions()
        {
            Scheme = "http",
            Host = "localhost",
            Port = Port.ToString(),
        };

        public Func<HttpContext, bool> IsConfiguredPath
        {
            get
            {
                return delegate (HttpContext httpContext)
                {
                    return httpContext.Request.Path.Value
                        .StartsWith(Pattern, StringComparison.OrdinalIgnoreCase);
                };
            }
        }
    }
}
