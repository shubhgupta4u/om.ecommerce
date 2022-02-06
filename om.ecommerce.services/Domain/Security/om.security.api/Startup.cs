using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using om.security.businesslogic;
using om.security.businesslogic.Interfaces;
using om.security.models;
using om.shared.api.common;

namespace om.ecommerce.security.api
{
    public class Startup: BaseStartup
    {
        public Startup(IConfiguration configuration):base(configuration)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<om.shared.caching.Interfaces.IUserTokenRepository, om.shared.caching.Repository.UserTokenRepository>();
            base.ConfigureServices(services);
            services.AddScoped<IAuthBusinessLogic, AuthBusinessLogic>();

            services.Configure<AccountApiEndPoints>(this.AppSettingConfigs.GetSection("AccountApiEndPoints"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.Configure(app, env);
        }
    }
}
