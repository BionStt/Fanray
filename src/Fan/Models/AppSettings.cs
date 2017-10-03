﻿using Fan.Enums;

namespace Fan.Models
{
    /// <summary>
    /// AppSettings section in appsettings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The database to use: "sqlite" (default) or "sqlserver".
        /// </summary>
        public ESupportedDatabase Database { get; set; } = ESupportedDatabase.Sqlite;
        
        /// <summary>
        /// The preferred domain to use: "auto" (default), "www", "nonwww".
        /// </summary>
        /// <remarks>
        /// - "auto" will just use whatever the url is given, will not do forward
        /// - "www" will forward root domain to www subdomain, e.g. fanray.com -> www.fanray.com
        /// - "nonwww" will forward www subdomain to root domain, e.g. www.fanray.com -> fanray.com
        /// 
        /// Note if you are running from a sub domian other than "www", preferred domain will be ignored.
        /// 
        /// This setting is for SEO, it's good to decide on a preferred domain as indicated in this
        /// Google Search Console document https://support.google.com/webmasters/answer/44231?hl=en
        /// </remarks>
        public EPreferredDomain PreferredDomain { get; set; } = EPreferredDomain.Auto;
        
        /// <summary>
        /// Whether to use https: false (default) or true.
        /// </summary>
        /// <remarks>
        /// - false, will not forward http to https
        /// - true, will forward http to https
        /// 
        /// This setting is for SEO, Google strongly recommend all website to use https.
        /// Note if user sets this value to false, but is using https, I don't downgrade you to http.
        /// </remarks>
        public bool UseHttps { get; set; }
    }
}
