using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaGestionVentas.Filters
{
    public class SessionAuthorizeAttribute : AuthorizeAttribute // va a heredar las funciones necesarias
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext) // este metodo es el filtro para dejar entrar o no al usuario
        {
            return httpContext.Session["UserId"] != null; // si existe una sesión abierta devuelve el id
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) // si no existe sesión, ejecuta
        {
            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Auth" },
                        { "action", "Login" }
                    });
        }
    }
}