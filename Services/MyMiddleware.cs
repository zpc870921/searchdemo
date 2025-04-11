using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace seachdemo.Services
{
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            this._next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        { 
            var password = context.Request.Query["password"].ToString();
            if (password == "123456")
            {
                if (context.Request.HasJsonContentType())
                {
                    var stream = context.Request.BodyReader.AsStream();
                    var jsonbody= await System.Text.Json.JsonSerializer.DeserializeAsync<dynamic>(stream);
                    //var jsonbody2 = await context.Request.ReadFromJsonAsync();
                    context.Items["JsonBody"] = jsonbody;
                }
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("没有权限");
                return;
            }
        }
    }
}
