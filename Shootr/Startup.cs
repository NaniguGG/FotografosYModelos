using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Shootr.Startup))]
namespace Shootr
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
