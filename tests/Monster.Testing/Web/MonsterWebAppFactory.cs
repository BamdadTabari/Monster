// tests/Monster.Testing/Web/MonsterWebAppFactory.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Monster.Testing.Web;

public abstract class MonsterWebAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            foreach (var (serviceType, impl) in Replacements())
            {
                services.RemoveAll(serviceType);
                services.AddSingleton(serviceType, impl);
            }
        });
    }

    // Return a dictionary of serviceType -> implementation to install for this test host
    protected abstract IEnumerable<(Type serviceType, object implementation)> Replacements();
}
