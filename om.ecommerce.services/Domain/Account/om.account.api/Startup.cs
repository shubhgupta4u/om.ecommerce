using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using om.account.businesslogic;
using om.account.businesslogic.Interfaces;
using om.account.repository;
using om.account.repository.Interfaces;
using om.shared.api.common;

namespace om.account.api
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<om.shared.caching.Interfaces.IUserTokenRepository, om.shared.caching.Repository.UserTokenRepository>();
            base.ConfigureServices(services);
            services.AddScoped<IUserBusinessLogic, UserBusinessLogic>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.Configure(app, env);

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
