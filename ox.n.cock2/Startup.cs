using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ox.n.cock2.Startup))]
namespace ox.n.cock2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
