using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyLayer.Formatters
{
    public class OctetOutputFormatter : TextOutputFormatter
    {
        public OctetOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/octet-stream"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(String).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;

            var response = context.HttpContext.Response;

            var buffer = new StringBuilder();
            if (context.Object is String)
            {
                String data = context.Object as String;
                buffer.Append(data);
            }
            return response.WriteAsync(buffer.ToString());
        }

    }
}
