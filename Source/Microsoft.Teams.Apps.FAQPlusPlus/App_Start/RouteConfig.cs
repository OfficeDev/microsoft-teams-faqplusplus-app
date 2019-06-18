﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    /// <summary>
    /// Router Config
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Register Routes
        /// </summary>
        /// <param name="routes">Routes collection</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
