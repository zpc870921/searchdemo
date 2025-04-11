using Mapster;
using Mapster.Utils;
using Microsoft.EntityFrameworkCore;
using seachdemo.Data;
using seachdemo.Mappers;
using seachdemo.Models;
using seachdemo.Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace seachdemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

           
            builder.Services.ConfigureMapster();
            builder.Services.AddMapster();
            builder.Services.AddMemoryCache();
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient<CustomerSubService>();
            builder.Services.AddDbContext<MyDbContext>(options =>
            {
                var conn = builder.Configuration.GetConnectionString("shopdb");
                options.UseMySql(conn,ServerVersion.AutoDetect(conn));
            });
            builder.Services.AddCap(x =>
            {
                x.GroupNamePrefix = "searchsvc";
                x.UseRabbitMQ(opt =>
                {
                    opt.HostName = "172.24.210.0";  // RabbitMQ服务器地址
                    opt.VirtualHost = "/";          // 与Canal配置中的virtual.host一致
                    opt.UserName = "canal";         // 用户名
                    opt.Password = "canal";         // 密码
                });
                // 启用仪表盘（可选）
                x.UseDashboard();

                // 重试配置
                x.FailedRetryCount = 5;             // 失败重试次数
                x.FailedRetryInterval = 60;         // 失败重试间隔(秒)

                x.UseEntityFramework<MyDbContext>();
            });

            builder.Services.AddElasticSearch();
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            //app.UseMiddleware<MyMiddleware>();
            //app.UseResponseCaching();
            app.MapControllers();

            app.Run();
        }
    }
}
