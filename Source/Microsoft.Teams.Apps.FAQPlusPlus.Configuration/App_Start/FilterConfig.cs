﻿using System.Web;
using System.Web.Mvc;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
