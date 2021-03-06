﻿using Fan.Widgets;

namespace Fan.Web.Widgets.BlogArchives
{
    public class BlogArchivesWidget : Widget
    {
        public BlogArchivesWidget()
        {
            Title = "Archives";
            ShowPostCount = true;
        }

        /// <summary>
        /// Whether to show post count next to archive month.
        /// </summary>
        public bool ShowPostCount { get; set; }
    }
}
