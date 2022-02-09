using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using myDamco.Database;
using Yahoo.Yui.Compressor;

namespace myDamco.Config
{
    /** This class encapsulates the settings from the database table "Settings". It also handles caching of the settings, so that the database will not be accessed each time a setting is requested
     *  (this is to decrease the db-load in case of many concurrent users). 
     * 
     *  Note: Since the myDamco production environment runs on two independent web-servers, which can only communicate through the database, it is not possible to clear the cache when a value is changed 
     *        (one server cannot clear the other servers cache, only its own). The values in the cache therefore has to time out, which means that changes to the settings does not take effect instantly. 
     *        A reasonable short timeout-period has been chosen, so that the delay between changing a setting in the admin tool and having it take effect is not too long, but at the same time long enough
     *        so that the load on the database will not be impacted much when many users are online at the same time.
     *  TODO? Maybe it will be faster just to read _all_ values from the table each time a setting is accessed (and the cache has timed out), instead of multiple small single-setting DB-queries?
     */
    public class Settings
    {
        // Settings for Piwik
        public static class Piwik
        {
            private const string EnableTrackingName = "Piwik.Enable Tracking";
            private const string SiteIdName = "Piwik.SiteID";
            private const string ServerUrlName = "Piwik.ServerUrl";

            // Get or set whether piwik tracking is enabled. Throws exception if a DB error occurs.
            public static bool TrackingEnabled 
            {
                get{ return GetSetting(EnableTrackingName).Trim().EqualsIgnoreCase("true"); }
                set{ SetSetting(EnableTrackingName, value ? "true" : "false"); }
            }


            // Get or set SiteID of myDamco on the piwik server, or -1 if none defined. Throws exception if a DB error occurs.
            public static int SiteId
            {
                get
                {
                    string s = GetSetting(SiteIdName, onRemoveCallback: (k, v, r) => RemoveFromCache(ServerUrlName)).Trim(); // <- when siteId is removed/timedout from cache, the serverUrl value must be removed too, so they're in sync after being changed. (otherwise we could send data to the wrong site on the piwik server)
                    int i;
                    if (Int32.TryParse(s, out i)) return i;
                    return -1;
                }
                set { SetSetting(SiteIdName, value.ToString()); }
            }

            // Get or set server url of the piwik server (starts with "://" not "http://" or "https://"). Throws exception if a DB error occurs.
            public static string ServerUrl
            {
                get { return GetSetting(ServerUrlName, onRemoveCallback: (k, v, r) => RemoveFromCache(SiteIdName)).Trim(); } // <- when serverUrl is removed/timedout from cache, the siteId value must be removed too, so they're in sync after being changed. (otherwise we could send data to the wrong site on the piwik server)
                set { SetSetting(ServerUrlName, value); }                
            }

        }

        // Settings for Google Analytics.
        public static class GoogleAnalytics
        {
            private const string EnableTrackingName = "Google Analytics.Enable Tracking";
            private const string TrackingIdName = "Google Analytics.Tracking ID";

            // Get or set whether tracking by Google Analytics is enabled. Throws exception if a DB error occurs.
            public static bool TrackingEnabled
            {
                get { return GetSetting(EnableTrackingName).Trim().EqualsIgnoreCase("true"); }
                set { SetSetting(EnableTrackingName, value ? "true" : "false"); }
            }

            // Get or set tracking ID for Google Analytics. Throws exception if a DB error occurs.
            public static string TrackingId
            {
                get { return GetSetting(TrackingIdName).Trim(); } // <- when serverUrl is removed/timedout from cache, the siteId value must be removed too, so they're in sync after being changed. (otherwise we could send data to the wrong site on the piwik server)
                set { SetSetting(TrackingIdName, value); }
            }
        }

        // Settings for WalkMe.
        public static class WalkMe
        {
            private const string EnableTrackingName = "WalkMe.Enable Tracking";
            private const string SourceUrlName = "WalkMe.Source";

            // Get or set whether tracking by WalkMe is enabled. Throws exception if a DB error occurs.
            public static bool TrackingEnabled
            {
                get { return GetSetting(EnableTrackingName).Trim().EqualsIgnoreCase("true"); }
                set { SetSetting(EnableTrackingName, value ? "true" : "false"); }
            }

            // Get or set tracking ID for Google Analytics. Throws exception if a DB error occurs.
            public static string SourceUrl
            {
                get { return GetSetting(SourceUrlName).Trim(); } // <- when serverUrl is removed/timedout from cache, the siteId value must be removed too, so they're in sync after being changed. (otherwise we could send data to the wrong site on the piwik server)
                set { SetSetting(SourceUrlName, value); }
            }
        }

        // Settings for logging
        public static class Logging
        {
            private const string EnableClientLoggingName = "Logging.EnableClientLogging";

            // Get or set whether logging of client-side JavaScript errors is enabled. Throws exception if a DB error occurs.
            public static bool ClientLoggingEnabled
            {
                get { return GetSetting(EnableClientLoggingName).Trim().EqualsIgnoreCase("true"); }
                set { SetSetting(EnableClientLoggingName, value ? "true" : "false"); }
            }
        }

        // Settings for navigation menu
        public static class Navigation
        {
            private const string CompressExternalMenuJavaScriptName = "Navigation.CompressExternalMenuJavaScript";

            // Get or set whether our own javascript for the external menu should be compressed. Throws exception if a DB error occurs.
            public static bool CompressExternalMenuJavaScript
            {
                get { return GetSetting(CompressExternalMenuJavaScriptName).Trim().EqualsIgnoreCase("true"); }
                set { SetSetting(CompressExternalMenuJavaScriptName, value ? "true" : "false"); }
            }
        }

        // The cache is global, so have to use some string to separate our keys from the keys inserted into it by other parts of the system...
        private const string cacheKeyPrefix = "Settings.cs:Setting:";

        // Returns the value for a setting. Throws exception if the setting name is not found (or other errors occur).
        private static string GetSetting(string settingName, int cacheTimeout = 30, bool forceDBFetch = false, CacheItemRemovedCallback onRemoveCallback = null)
        {
            string cacheKey = cacheKeyPrefix + settingName;
            string cacheValue = HttpRuntime.Cache.Get(cacheKey) as string;

            if (cacheValue != null && !forceDBFetch)
                return cacheValue;

            using (var myDamcoDB = new myDamcoEntities())
            {
                string value = myDamcoDB.Setting.Single(s => s.Name == settingName).Value;
                if (cacheTimeout > 0) HttpRuntime.Cache.Insert(cacheKey, value, null, DateTime.UtcNow.AddSeconds(cacheTimeout), Cache.NoSlidingExpiration, CacheItemPriority.Default, onRemoveCallback);
                else                  HttpRuntime.Cache.Remove(cacheKey);
                return value;
            }                
        }

        // Update the value for a setting. Throws exception if the setting name is not found (or other errors occur).
        private static void SetSetting(string settingName, string value)
        {
            try
            {
                using (var myDamcoDB = new myDamcoEntities())
                {
                    Setting setting = myDamcoDB.Setting.Single(s => s.Name == settingName);
                    setting.Value = value;
                    myDamcoDB.SaveChanges();
                }
            }
            finally
            {
                // Update the cache too. Note, on prod this will only update the cache on one of the two web-servers. The other server does not see the new value 
                // until the old value has timed out. (On dev, however, it is nice not to have to wait for timeout)
                // TODO: Make the admin tool use this method, so that it clears the cache too.
                // TODO: ...or find out if SqlCacheDependency can be used (whether it queryies the DB for each cache access (bad) or if it uses some clever notification method which makes it fast)
                string cacheKey = cacheKeyPrefix + settingName;
                HttpRuntime.Cache.Remove(cacheKey); // <- taking the safe approach (concurrency)
            }
        }

        private static void RemoveFromCache(string settingName)
        {
            HttpRuntime.Cache.Remove(cacheKeyPrefix + settingName);
        }
    }
}