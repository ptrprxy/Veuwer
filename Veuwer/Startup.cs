using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Veuwer.Startup))]
namespace Veuwer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
