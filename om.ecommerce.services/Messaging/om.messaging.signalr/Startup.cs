using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using om.messaging.signalr.Hubs;
using om.shared.api.middlewares;
using om.shared.api.middlewares.Filters;
using om.shared.logger;
using om.shared.logger.helpers;
using om.shared.logger.models;
using om.shared.security;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace om.messaging.signalr
{
    public class Startup
    {
        private const string ALLOW_SPECIFIC_ORIGINS = "_myAllowSpecificOrigins";
        private const string ALLOW_ANY_ORIGINS = "_anyAllowOrigins";
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            ApplicationName = this.GetApplicationName();
            this.Version = this.GetApplicationVersion();
            this.AuthSettingConfigs = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine("Settings", "authsettings.json")).Build();
            this.LogSettingConfigs = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine("Settings", "logsettings.json")).Build();
        }

        public IConfiguration AuthSettingConfigs { get; }
        public IConfiguration LogSettingConfigs { get; }
        public IConfiguration Configuration { get; }
        public string ApplicationName { get; }
        public string Version { get; }
        public IAuthService AuthService { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtSetting>(this.AuthSettingConfigs.GetSection("JwtSetting"));

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            var redisEndPoint = this.Configuration.GetSection("RedisEndPoint").Value;
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisEndPoint;
            });
            services.AddScoped<om.shared.caching.Interfaces.ISignalRConnectionRepository, om.shared.caching.Repository.SignalRConnectionRepository>();

            services.AddScoped<om.shared.caching.Interfaces.IUserTokenRepository, om.shared.caching.Repository.UserTokenRepository>();
            services.AddScoped<IAuthService, AuthService>();

            string securityKey = this.AuthSettingConfigs.GetSection("JwtSetting:SecurityKey").Value;
            this.AuthService = services.BuildServiceProvider(false).GetRequiredService<IAuthService>();

            var tokenKey = Encoding.ASCII.GetBytes(securityKey);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = this.AuthService.GetTokenValidationParameters();

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var te = context.Exception;
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var path = context.HttpContext.Request.Path;
                            Microsoft.Extensions.Primitives.StringValues authTokens;
                            authTokens = context.Request.Query["access_token"];
                            if (!authTokens.Any())
                            {
                                context.Request.Headers.TryGetValue("Authorization", out authTokens);
                            }
                            string accessToken = authTokens.FirstOrDefault();
                           
                            if (path.StartsWithSegments("/hub") && accessToken != null)
                            {
                                if (accessToken.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    accessToken = accessToken.Substring("Bearer ".Length);
                                }
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;

                        }
                    };
                });

            //Loging Configuration and setup
            var appInsightInstrumentationKey = this.LogSettingConfigs.GetSection("AppInsightInstrumentationKey");
            if (appInsightInstrumentationKey != null)
            {
                services.AddApplicationInsightsTelemetry(appInsightInstrumentationKey.Value);
            }
            services.Configure<LogSetting>((option) =>
            {
                option.ApplicationName = this.ApplicationName;
                option.Sink = this.LogSettingConfigs.GetSection("Sink").Get<SinkLog>();
                option.AppInsightInstrumentationKey = appInsightInstrumentationKey.Value;
                option.SinkConfiguration = LogSinkHelper.GetSinkConfiguration(option.Sink, option.ApplicationName);
            });
            services.AddSingleton<om.shared.logger.Interfaces.ILogger, om.shared.logger.Logger>();

            services.AddHttpClient();

            string[] allowedOrigins = this.AuthSettingConfigs.GetSection("JwtSetting:AllowedOrigins").Get<string[]>();
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
            services.AddMvc(
                   option =>
                   {
                       option.EnableEndpointRouting = false;
                       option.Filters.Add<ExceptionFilter>();
                   }
               );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<LogContextEnrichment>();
            app.UseLoggerMiddleware();
            app.UseMiddleware<LogContextEnrichment>();
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

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/hub/order");
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("SignalR Service Api is up and running...");
                });
            });
        }
        #region Private Methods
        private string GetApplicationName()
        {
            var appNameSection = this.Configuration.GetSection("ApplicationName");
            if (appNameSection != null && !string.IsNullOrEmpty(appNameSection.Value))
            {
                return appNameSection.Value;
            }
            else
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }
        private string GetApplicationVersion()
        {
            var appNameSection = this.Configuration.GetSection("ApiVersion");
            if (appNameSection != null && !string.IsNullOrEmpty(appNameSection.Value))
            {
                return appNameSection.Value;
            }
            else
            {
                return "v1";
            }
        }
        #endregion
    }
}
