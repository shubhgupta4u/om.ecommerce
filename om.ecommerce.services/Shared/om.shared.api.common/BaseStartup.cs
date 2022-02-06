using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using om.shared.api.common.Interfaces;
using om.shared.api.common.Services;
using om.shared.api.middlewares;
using om.shared.api.middlewares.Filters;
using om.shared.caching.Interfaces;
using om.shared.dataaccesslayer;
using om.shared.logger;
using om.shared.logger.helpers;
using om.shared.logger.models;
using om.shared.security;
using om.shared.security.Interfaces;
using om.shared.security.models;
using System.Reflection;

namespace om.shared.api.common
{
    public abstract class BaseStartup: om.shared.api.common.Interfaces.IStartup
    {
        private const string ALLOW_SPECIFIC_ORIGINS = "_myAllowSpecificOrigins";
        private const string ALLOW_ANY_ORIGINS = "_anyAllowOrigins";

        public BaseStartup(IConfiguration configuration)
        {
            AppSettingConfigs = configuration;
            ApplicationName = this.GetApplicationName();
            this.Version = this.GetApplicationVersion();
            this.AuthSettingConfigs = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine("Settings","authsettings.json")).Build();
            this.LogSettingConfigs = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine("Settings", "logsettings.json")).Build();
        }

        #region
        public IConfiguration AppSettingConfigs { get; }
        public IConfiguration AuthSettingConfigs { get; }
        public IConfiguration LogSettingConfigs { get; }
        public string ApplicationName { get; }
        public string Version { get; }
        #endregion

        #region om.shared.api.common.Interfaces.IStartup
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMongoBookDBContext, MongoBookDBContext>();
            services.AddSingleton<om.shared.logger.Interfaces.ILogger, om.shared.logger.Logger>();

            var appInsightInstrumentationKey = this.LogSettingConfigs.GetSection("AppInsightInstrumentationKey");
            if (appInsightInstrumentationKey != null)
            {
                services.AddApplicationInsightsTelemetry(appInsightInstrumentationKey.Value);
            }

            var redisEndPoint = AppSettingConfigs.GetSection("RedisEndPoint").Value;
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisEndPoint;
            });

            services.Configure<LogSetting>((option)=>
            {
                option.ApplicationName = this.ApplicationName;
                option.Sink = this.LogSettingConfigs.GetSection("Sink").Get<SinkLog>();
                option.AppInsightInstrumentationKey = appInsightInstrumentationKey.Value;
                option.SinkConfiguration = LogSinkHelper.GetSinkConfiguration(option.Sink, option.ApplicationName);
            });
            
            services.Configure<AuthSettings>(this.AuthSettingConfigs);
            services.Configure<OktaSetting>(this.AuthSettingConfigs.GetSection("OktaSetting"));
            services.Configure<AzureAdSetting>(this.AuthSettingConfigs.GetSection("AzureAdSetting"));
            services.Configure<JwtSetting>(this.AuthSettingConfigs.GetSection("JwtSetting"));

            IAuthService authService = services.BuildServiceProvider(false).GetRequiredService<IAuthService>();
            authService.RegisterAuthentication(services);
            AuthorizeAttribute.RegisterAuthService(authService);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = this.ApplicationName, Version = this.Version });
            });
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
            services.Configure<Mongosettings>(options =>
            {
                options.Connection = AppSettingConfigs.GetSection("Mongosettings:Connection").Value;
                options.DatabaseName = AppSettingConfigs.GetSection("Mongosettings:DatabaseName").Value;
            });
        }
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", string.Format("{0} {1}", this.ApplicationName, this.Version)));
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
            });
        }
        #endregion

        #region Private Methods
        private string GetApplicationName()
        {
            var appNameSection = this.AppSettingConfigs.GetSection("ApplicationName");
            if(appNameSection != null && !string.IsNullOrEmpty(appNameSection.Value))
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
            var appNameSection = this.AppSettingConfigs.GetSection("ApiVersion");
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
