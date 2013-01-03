using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Pipelines.InsertRenderings;
using Sitecore.Data.Fields;
using Sitecore;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Layouts;
using HedgehogDevelopment.SitecoreMvcAspxInterop.Controls;
using Sitecore.Mvc.Presentation;
using System.Web.Routing;
using Sitecore.Mvc.Configuration;

namespace HedgehogDevelopment.SitecoreMvcAspxInterop.Pipelines
{
    public class WrapMvcRendering : InsertRenderingsProcessor
    {
        public override void Process(InsertRenderingsArgs args)
        {
            //Get the merged item layout field value
            Field layoutField = args.ContextItem.Fields[FieldIDs.LayoutField];
            if (layoutField == null)
            {
                return;
            }

            string fieldValue = LayoutField.GetFieldValue(layoutField);
            if (string.IsNullOrEmpty(fieldValue))
            {
                return;
            }

            //Parse it for easier processing
            XDocument layout = XDocument.Parse(fieldValue);

            XmlBasedRenderingParser parser = new XmlBasedRenderingParser();

            //Build a shared page context
            Sitecore.Mvc.Presentation.PageContext pageCtx = new Sitecore.Mvc.Presentation.PageContext();

            HttpContextWrapper httpCtxWrapper = new HttpContextWrapper(HttpContext.Current);
            RouteData routeData = CreateRouteData();

            pageCtx.RequestContext = CreateRequestContext(httpCtxWrapper, routeData);

            string deviceId = Sitecore.Context.Device.ID.ToString();

            //Loop through the renderings
            foreach (RenderingReference renderingReference in args.Renderings)
            {
                string templateName = renderingReference.RenderingItem.InnerItem.TemplateName;

                //Only do the replacement for MVC renderings
                if (templateName == "View rendering" || templateName == "Controller rendering" || templateName == "Item rendering")
                {
                    XElement renderingXml = layout.XPathSelectElement("/r/d[@id='" + deviceId + "']/r[@id='" + renderingReference.RenderingID + "']");
                    Rendering rendering = parser.Parse(renderingXml, false);

                    renderingReference.SetControl(new RenderingWrapper(rendering, pageCtx));
                }
            }
        }

        /// <summary>
        /// Creates a RouteData with enough information for Sitecore to do what it needs.
        /// </summary>
        /// <returns></returns>
        private static RouteData CreateRouteData()
        {
            RouteData routeData = new RouteData();
            routeData.Values["scLanguage"] = Sitecore.Context.Language.Name;
            routeData.Values["controller"] = MvcSettings.SitecoreControllerName;

            return routeData;
        }

        /// <summary>
        /// Creates a request context for mvc renderings
        /// </summary>
        /// <param name="httpCtxWrapper"></param>
        /// <param name="routeData"></param>
        /// <returns></returns>
        private RequestContext CreateRequestContext(HttpContextWrapper httpCtxWrapper, RouteData routeData)
        {
            RequestContext requestCtx = new RequestContext();
            requestCtx.HttpContext = httpCtxWrapper;
            requestCtx.RouteData = routeData;

            return requestCtx;
        }
    }
}