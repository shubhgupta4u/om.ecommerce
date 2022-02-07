using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using System;
using System.IO;
using System.Net;

namespace om.gateway.ocelot.api
{
    public class Startup
    {
        private const string ALLOW_SPECIFIC_ORIGINS = "_myAllowSpecificOrigins";
        private const string ALLOW_ANY_ORIGINS = "_anyAllowOrigins";
        private readonly IConfiguration ocelotConfigs;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.ocelotConfigs = new ConfigurationBuilder().AddJsonFile("Ocelot.json").Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerForOcelot(this.ocelotConfigs);
            services
                .AddOcelot(this.ocelotConfigs)
                .AddPolly();
                //.AddConsul();

            services.AddControllers();
            string[] allowedOrigins = this.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            services.AddCors(options =>
            {
                options.AddPolicy(name: ALLOW_ANY_ORIGINS,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                                  });
                if (allowedOrigins != null)
                {
                    options.AddPolicy(name: ALLOW_SPECIFIC_ORIGINS,
                                  builder =>
                                  {
                                      builder.WithOrigins(allowedOrigins)
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials();
                                  });
                }
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ocelot", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }           
            app.UseHttpsRedirection();

            app.UseRouting();
            if (env.IsDevelopment())
            {
                app.UseCors(ALLOW_ANY_ORIGINS);
            }
            else
            {
                app.UseCors(ALLOW_SPECIFIC_ORIGINS);
            }

            app.UseSwaggerForOcelotUI(opt => {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            }).UseOcelot().Wait();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context => {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }

}
