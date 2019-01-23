using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyLayer.Middlewares
{
    public static class RequestLoggerExtensions
    {
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogger>();
        }
    }

    class RequestLogger
    {
        private readonly RequestDelegate Next;
        public RequestLogger(RequestDelegate Next)
        {
            this.Next = Next;
        }

        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine();

            Console.WriteLine("{");

            DateTime sgDateTime = DateTime.Now.AddHours(8); // To Get Singapore Time
            Console.WriteLine("\"Time\" : \"{0}\",", sgDateTime.ToString("R"));

            Console.WriteLine("\"UserAgent\" : \"{0}\",", context.Request.Headers["User -Agent"].ToString());

            Console.WriteLine("\"HostIp\" : \"{0}\",", context.Connection.RemoteIpAddress);
            Console.WriteLine("\"HostPort\" : \"{0}\",", context.Connection.RemotePort);

            Console.WriteLine("\"Method\":\"{0}\",", context.Request.Method);

            Console.WriteLine("\"QueryString\": \"{0}\",", context.Request.Path.ToString());

            Console.WriteLine("\"Authorization\": \"{0}\",", context.Request.Headers["Authorization"]);

            Console.WriteLine("\"ContentType\": \"{0}\"", context.Request.ContentType);

            // TODO : Review Reading body
            //Console.WriteLine("Body:");
            //var requestData = new System.IO.StreamReader(context.Request.Body).ReadToEnd();
            //Console.WriteLine(requestData);

            //context.Request.Body = new System.IO.MemoryStream();
            //System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(context.Request.Body);
            //streamWriter.Write(requestData);
            //streamWriter.Flush();
            //context.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);

            Console.WriteLine("}");

            await Next.Invoke(context);
        }
    }
}
