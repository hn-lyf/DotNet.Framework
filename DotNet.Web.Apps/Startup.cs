using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Web.Apps.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace DotNet.Web.Apps
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }
        /// <summary>
        /// 配置信息
        /// </summary>
        public static IConfiguration Configuration { get; private set; }
        /// <summary>
        /// 网站启动信息
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment { get; private set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Swagger基础配置
            if (Configuration.GetSection("Swagger").Exists())//如果swagger 存在就配置
            {
                services.AddSwaggerGen(c =>
                {
                    var info = new OpenApiInfo();
                    Configuration.GetSection("Swagger").Bind(info);
                    c.SwaggerDoc(Configuration["Swagger:Name"], info);
                    foreach (string file in System.IO.Directory.GetFiles(WebHostEnvironment.ContentRootPath, "*.xml"))
                    {
                        c.IncludeXmlComments(file, true);
                    }
                    c.AddSecurityDefinition(Configuration.GetSection("Session:HeaderName").Value, new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = $"请输入登录返回的{Configuration.GetSection("Session:HeaderName").Value}",
                        Name = Configuration.GetSection("Session:HeaderName").Value,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement() {   {
                            new OpenApiSecurityScheme{
                                Reference = new OpenApiReference {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = Configuration.GetSection("Session:HeaderName").Value}
                           },new string[] { }
                        } });
                });

            }
            #endregion

            services.AddDistributedRedisCache(options =>
            {
                Configuration.GetSection("Session:RedisCache").Bind(options);
            });
            services.AddSession((o) =>
            {
                o.Cookie.Name = "uuid";
                o.IdleTimeout = TimeSpan.FromHours(24);
            });
            services.AddConsulConfig(Configuration);
            services.AddControllersWithViews((options) =>
            {
                options.Filters.Add(new Filters.ApiLogAsyncActionFilter());
                options.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
                options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.Converters.Add(new DotNet.LongJsonConverter());
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                Newtonsoft.Json.JsonConvert.DefaultSettings = () => options.SerializerSettings;
                ;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseConsul();
            app.UseStaticFiles();
            app.UseMySession();
            app.UseRouting();
            app.UseAuthorization();

            #region 启用Swagger
            if (Configuration.GetSection("Swagger").Exists())//如果swagger 存在就配置
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {

                    options.DefaultModelsExpandDepth(-1);
                    options.RoutePrefix = "help";
                    options.DocumentTitle = Configuration["Swagger:Description"];
                    options.SwaggerEndpoint("../swagger/" + Configuration["Swagger:Name"] + "/swagger.json", Configuration["Swagger:Description"]);
                });

            }
            #endregion
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                      name: "areas",
                      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
