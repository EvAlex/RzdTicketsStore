using Microsoft.Owin;
using Owin;
using RzdTicketsStore.Models;

[assembly: OwinStartupAttribute(typeof(RzdTicketsStore.Startup))]
namespace RzdTicketsStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var db = new RzdTicketsDb();
            db.Initialize();

            ConfigureAuth(app);
        }
    }
}
