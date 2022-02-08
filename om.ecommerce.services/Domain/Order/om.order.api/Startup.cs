using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using om.order.api.SignalRClients;
using om.shared.api.common;
using om.shared.signalr;

namespace om.ecommerce.order.api
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<om.shared.caching.Interfaces.IUserTokenRepository, om.shared.caching.Repository.UserTokenRepository>();
            base.ConfigureServices(services);
            services.Configure<SignalRSetting>((op)=> {
                op.BaseUrl = this.SignalRSettingConfigs.GetSection("BaseUrl").Value;
                op.JwtBearerToken = this.AuthService.GenerateApiServiceTokenAsync(this.ApplicationName,System.DateTime.Now.AddDays(365)).Result;
            });
            services.AddScoped<IHubClient, OrderHubClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.Configure(app, env);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
