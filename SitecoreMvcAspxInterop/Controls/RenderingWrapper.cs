using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Sitecore.Layouts;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web.Routing;
using Sitecore.Mvc.Configuration;
using Sitecore.Mvc.Presentation;
using Sitecore.Mvc.Common;
using System.Web.Mvc;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Sitecore.Mvc.Pipelines;
using Sitecore.Web.UI;

namespace HedgehogDevelopment.SitecoreMvcAspxInterop.Controls
{
    public class RenderingWrapper : WebControl
    {
        Rendering _rendering;
        Sitecore.Mvc.Presentation.PageContext _pageCtx;

        public RenderingWrapper(Rendering rendering, Sitecore.Mvc.Presentation.PageContext pageCtx)
        {
            _rendering = rendering;
            _pageCtx = pageCtx;
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            HttpContextWrapper httpCtxWrapper = _pageCtx.RequestContext.HttpContext as HttpContextWrapper;

            if (httpCtxWrapper != null)
            {
                using (ContextService.Get().Push<Sitecore.Mvc.Presentation.PageContext>(_pageCtx))
                {
                    ViewContext viewCtx = CreateViewContext(httpCtxWrapper, _pageCtx.RequestContext.RouteData, _rendering);

                    using (ContextService.Get().Push<ViewContext>(viewCtx))
                    {
                        PipelineService.Get().RunPipeline<RenderRenderingArgs>("mvc.renderRendering", new RenderRenderingArgs(_rendering, output));
                    }
                }
            }
            else
            {
                throw new Exception("Invalid HttpContextWrapper");
            }
        }

        /// <summary>
        /// Create an empty view context for rendering the Sitecore rendering
        /// </summary>
        /// <param name="httpCtxWrapper"></param>
        /// <param name="routeData"></param>
        /// <param name="rendering"></param>
        /// <returns></returns>
        private static ViewContext CreateViewContext(HttpContextWrapper httpCtxWrapper, RouteData routeData, Rendering rendering)
        {
            ViewContext viewCtx = new ViewContext();
            viewCtx.ViewData = new ViewDataDictionary();
            viewCtx.View = new RenderingView(rendering);
            viewCtx.TempData = new TempDataDictionary();
            viewCtx.RouteData = routeData;
            viewCtx.HttpContext = httpCtxWrapper;
            return viewCtx;
        }
    }
}