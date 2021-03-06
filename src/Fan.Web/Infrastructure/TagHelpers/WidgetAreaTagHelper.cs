﻿using Fan.Widgets;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Fan.Web.TagHelpers
{
    /// <summary>
    /// A Tag Helper that renders a widget area, requires a valid area id.
    /// </summary>
    /// <remarks>
    /// TODO there should tag attributes that allow user to specify what html tag to surround the
    /// area, right now I'm hard coding a div. Also a css class to attach to the area.
    /// </remarks>
    [HtmlTargetElement("widget-area", Attributes = "id")]
    public class WidgetAreaTagHelper : TagHelper
    {
        /// <summary>
        /// Used to invoke ViewComponent. This injected helper is "neutral", not specific for our 
        /// view, so we have to "contextualize" it for the current view ViewContext before using.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components?view=aspnetcore-2.2#perform-synchronous-work
        /// https://github.com/aspnet/Mvc/issues/5504#issuecomment-258671545
        /// </remarks>
        private readonly IViewComponentHelper viewComponentHelper;
        private readonly IWidgetService widgetService;

        public WidgetAreaTagHelper(IViewComponentHelper viewComponentHelper,
            IWidgetService widgetService)
        {
            this.viewComponentHelper = viewComponentHelper;
            this.widgetService = widgetService;
        }

        /// <summary>
        /// The area id, bound to tag attribute.
        /// </summary>
        [HtmlAttributeName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Initializes the ViewContext of the executing page.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Outputs widget area html or nothing if the given widget area id is not found.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "widgets");

            ((IViewContextAware)this.viewComponentHelper).Contextualize(ViewContext);

            var area = await widgetService.GetAreaAsync(Id);
            if (area == null) return;

            for (int i = 0; i < area.WidgetIds.Length; i++)
            {
                var widgetIns = area.WidgetInstances[i];
                var widget = area.Widgets[i];

                var content = await viewComponentHelper.InvokeAsync(widgetIns.Folder, widget);
                output.Content.AppendHtml(content.GetString());
            }
        }
    }
}
