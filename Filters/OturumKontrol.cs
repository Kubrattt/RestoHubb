using System.Web.Mvc;

namespace RestoHub.Filters
{
    public class OturumKontrol : ActionFilterAttribute
    {
        // Filters/OturumKontrol.cs — güncelle
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // [OturumGerekmez] attribute'u varsa geç
            var aksiyonBesleme = filterContext.ActionDescriptor;
            bool serbestMi = aksiyonBesleme.IsDefined(typeof(OturumGerekmez), false)
                          || aksiyonBesleme.ControllerDescriptor.IsDefined(typeof(OturumGerekmez), false);

            if (serbestMi)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (filterContext.HttpContext.Session["KullaniciId"] == null)
            {
                filterContext.Result = new RedirectResult("/Account/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}