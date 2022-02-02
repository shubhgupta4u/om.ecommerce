using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using om.security.businesslogic;
using om.security.businesslogic.Interfaces;
using om.security.models;
using om.shared.security;
using om.shared.security.Interfaces;
using om.shared.security.models;

namespace om.ecommerce.security.api
{
    public class Startup
    {
        private const string ALLOW_SPECIFIC_ORIGINS = "_myAllowSpecificOrigins";
        private const string ALLOW_ANY_ORIGINS = "_anyAllowOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthBusinessLogic, AuthBusinessLogic>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();

            var redisEndPoint = Configuration.GetSection("RedisEndPoint").Value;
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisEndPoint;
            });

            services.Configure<OktaSetting>(Configuration.GetSection("OktaSetting")); 
            services.Configure<AzureAdSetting>(Configuration.GetSection("AzureAdSetting"));
            var jwtSetting = Configuration.GetSection("JwtSetting");
            services.Configure<JwtSetting>(jwtSetting);
            IAuthService authService = services.BuildServiceProvider(false).GetRequiredService<IAuthService>();
            authService.RegisterAuthentication(services);
            AuthorizeAttribute.RegisterAuthService(authService);

            var accountApiEndPoints = Configuration.GetSection("AccountApiEndPoints");
            services.Configure<AccountApiEndPoints>(accountApiEndPoints);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "om.ecommerce.security.api", Version = "v1" });
            });
            services.AddHttpClient();

            string[] allowedOrigins = Configuration.GetSection("JwtSetting:AllowedOrigins").Get<string[]>();
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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "om.ecommerce.security.api v1"));
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
