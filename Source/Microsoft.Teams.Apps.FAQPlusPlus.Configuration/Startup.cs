namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using global::Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var container = AutofacConfig.RegisterDependencies();
            this.ConfigureAuth(app, container);
        }
    }
}
