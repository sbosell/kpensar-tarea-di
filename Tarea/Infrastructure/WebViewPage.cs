using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Forloop.HtmlHelpers;
using Forloop;


namespace Tarea.Infrastructure
{
    public abstract class MyWebViewPage<T> : System.Web.Mvc.WebViewPage<T>
    {
        public Dictionary<string, object> PageVars = new Dictionary<string, object>();

        public override void ExecutePageHierarchy()
        {

            var ViewJs = this.VirtualPath.ToLower().Replace("~/views/", "~/content/script/page/").Replace(".cshtml", ".js");
            
            if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(ViewJs)))
            {
                Html.BeginScriptContext();
                Html.AddScriptFile(ViewJs);
                Html.EndScriptContext();

            }

            base.ExecutePageHierarchy();

            // if PageVars has values we'll add them to the global scope of the window and pageVar variable to be used.
            if (PageVars.Count > 0)
            {
                Html.BeginScriptContext();
                Html.AddScriptBlock(String.Format("var pageVar ={0};_.merge(window, pageVar);", Json.Encode(PageVars)));
                Html.EndScriptContext();
            }
        }
    }
}