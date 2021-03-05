using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Namei.Common.Api
{
  public class Config
  {
    public readonly string Env;

    public readonly string AppName;

    public readonly string AppVersion;

    public readonly string RcsDbConfig;

    public readonly string RcsUrl;

    public bool IsProduction { get => Env == "Production"; }

    public bool IsDevelopment { get => Env == "Development"; }

    public Config(IConfiguration config, IHostEnvironment env)
    {
      Env = env.EnvironmentName;
      AppName = config.GetValue<string>("app_name");
      AppVersion = config.GetValue<string>("app_version");
      RcsDbConfig = config.GetValue<string>("rcs.db.config");
      RcsUrl = config.GetValue<string>("rcs.url");
    }
  }
}