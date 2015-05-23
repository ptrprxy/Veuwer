using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Veuw.Startup))]
namespace Veuw
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
