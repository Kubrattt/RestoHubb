using RestoHub.Filters;
using System.Web.Mvc;

namespace RestoHub
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new OturumKontrol()); 
        }
    }
}