﻿using System.Web;
using System.Web.Mvc;

namespace Group19_iFINANCEAPP
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
