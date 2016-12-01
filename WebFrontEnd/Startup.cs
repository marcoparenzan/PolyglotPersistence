using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebFrontEnd.Startup))]
namespace WebFrontEnd
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
