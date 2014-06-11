using System.Web.Http;
using System.Web.Http.Cors;

class WebApiConfig
{
    public static void Register(HttpConfiguration configuration)
    {
        var cors = new EnableCorsAttribute("*", "*", "*");
        configuration.EnableCors(cors);
        
        configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}",
            new { id = RouteParameter.Optional });
    }
}