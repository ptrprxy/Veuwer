using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Veuwer.Startup))]
namespace Veuwer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
