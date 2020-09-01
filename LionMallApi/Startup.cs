using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Lion.Services;
using LionMall.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prod.Core;
using Swashbuckle.AspNetCore.Swagger;

namespace LionMallApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option =>
            {
                option.Filters.Add(new ApiResultFilter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "LionMall Api",
                    Version = "v1"
                });
                //var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHttpClient();
            services.AddSingleton<UserService>();
            services.AddSingleton<RegisterService>();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ChangerService>();
            services.AddSingleton<LangService>();
            services.AddSingleton<PaymentService>();
            services.AddSingleton<CommonService>();
            services.AddSingleton<PrepayService>();
            services.AddSingleton<RpcNotifyService>();
            services.AddSingleton<AssetService>();
            services.AddSingleton<OrderService>();
            services.AddSingleton<UserExtendService>();
            services.AddSingleton<MsgTool>();
            services.AddSingleton<IHttpUtils, HttpUtils>();
            services.AddSingleton<TicketService>();

            services.AddCors(options => options.AddPolicy("LionMall", p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<LionMallExceptionHandlerMiddleWare>();
            }
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LionMall Api v1");
                c.RoutePrefix = "";
            });
            app.UseCors("LionMall");   
        }
    }
}
