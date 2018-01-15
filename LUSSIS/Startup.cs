using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LUSSIS.Startup))]
namespace LUSSIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
