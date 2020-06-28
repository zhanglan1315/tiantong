using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Renet.Utils;
using Renet.Web;
using Tiantong.Account.Utils;

namespace Tiantong.Account.Api
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();

      services.AddSingleton<Config>();
      services.AddSingleton<DbBuilder>();
      services.AddSingleton<IHash, Hash>();
      services.AddSingleton<IRandom, Random>();
      services.AddSingleton<JWT>();
      services.AddSingleton<Mail>();

      services.AddHttpClient<EmailVerificationService>();

      services.AddScoped<AccountContext>();
      services.AddScoped<MigratorProvider>();

    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseMiddleware<JsonBody>();
      app.UseProvider<ExceptionHandler>();
      app.UseRouting();
      app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }
}
