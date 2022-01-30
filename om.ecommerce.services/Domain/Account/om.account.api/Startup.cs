using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using om.account.businesslogic;
using om.account.businesslogic.Interfaces;
using om.account.repository;
using om.account.repository.Interfaces;
using om.security.models;
using om.shared.dataaccesslayer;
using om.shared.security;
using om.shared.security.Interfaces;
using om.shared.security.models;

namespace om.account.api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserBusinessLogic, UserBusinessLogic>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
            services.AddScoped<IMongoBookDBContext, MongoBookDBContext>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();

            var redisEndPoint = Configuration.GetSection("RedisEndPoint").Value;
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisEndPoint;
            });

            var jwtSetting = Configuration.GetSection("JwtSetting");
            services.Configure<JwtSetting>(jwtSetting);
            IAuthService authService = services.BuildServiceProvider(false).GetRequiredService<IAuthService>();
            authService.RegisterAuthentication(services);
            AuthorizeAttribute.RegisterAuthService(authService);

            services.AddControllers();
            services.Configure<Mongosettings>(options =>
            {
                options.Connection = Configuration.GetSection("Mongosettings:Connection").Value;
                options.DatabaseName = Configuration.GetSection("Mongosettings:DatabaseName").Value;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "om.ecommerce.account.api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "om.ecommerce.account.api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
