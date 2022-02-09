using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Argotic.Syndication;
using myDamco.Database;
using myDamco.Utils;

namespace myDamco.Models
{
    public class NewsPageModel
    {
        public List<NewsPageItem> menuNewsItems;
        public NewsPageItem newsItem; // null for none
        public NewsCategory newsCategory;
        public bool showArchivedItemsInMenu;
    }

    public class NewsPageItem // TODO: Or use RssItem instead?
    {
        public string Url;
        public string Title;
        public string Body;
        public DateTime From;
        public DateTime? To;
        public bool External;

        // TODO: maybe have a string ID instead of an URL? (Can see on External which action method to call)

        public NewsPageItem(NewsItem newsItem, HttpRequestBase request)
        {
            Uri appUrl = new Uri(ControllerUtil.GetSitePathPath(false));  // Uri appUrl = new Uri(String.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, request.ApplicationPath));
            Url = String.Format("{0}/{1}", appUrl, "Dashboard/News/" + newsItem.Id);
            Title = newsItem.Title;
            Body = newsItem.Body;
            From = newsItem.From;
            To = newsItem.To;
            External = false;
        }

        public NewsPageItem(RssItem rssItem, int categoryId, HttpRequestBase request)
        {
            string newsLink = rssItem.Link.ToString().Replace("\"", "").Replace("\n", "");  // The Replace avoids XSS attacks from the link in the <a href="[link]"> tag.
            Uri appUrl = new Uri(ControllerUtil.GetSitePathPath(false)); // Uri appUrl = new Uri(String.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, request.ApplicationPath));
            //Url = String.Format("{0}/{1}", appUrl, "Dashboard/NewsExternal/" + EncodeExternalNewsItemId(categoryId, newsLink));
            Url = String.Format("{0}/{1}", appUrl, "Dashboard/NewsExternal/" + EncodeExternalNewsItemId(categoryId, rssItem.PublicationDate, rssItem.Title));
            //Url = rssItem.Link.ToString();
            Title = rssItem.Title;
            Body = "<p>To read this news post, <a href=\"" + newsLink + "\" target=\"_blank\">click here</a>.</p>";
            From = rssItem.PublicationDate;
            To = null;
            External = true;
        }

        // TODO: Return null if error/exception instead of throwing an exception (crashing)?
        // decode the base64 encoded id into a title and a DateTime
        // (Needed a unique id for an item in a external feed (not from the DB) to be able to show the body of the rss on the newspage, when clicking it. RSS-items doesn't (usually) have a unique ID.
        //  I chose to use the concatenation of the item's title and date as the unique id. It is unlikely that two items in the feed both has same title and date. Also i need the category-id, to look
        //  up the feed (to show the left-menu on the news-page, basically). The resulting string is encoded to base64 so that it can be put in URLs.)
        // TODO! I could have just used the URL. That is much simpler and should be 100% unique! Doh. (But then i need to add it to my NewsPageItem bean. Could add it as an ID maybe - a string which is a number for internal news, url for external)
        // TODO: ...actually damco has a few feeds with the same title and date...
        public static Tuple<int,DateTime,string> DecodeExternalNewsItemId(string id)
        {
            string idDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(id));
            int splitIndex1 = idDecoded.IndexOf("#");
            int categoryId = Convert.ToInt32(idDecoded.Substring(0, splitIndex1));
            int splitIndex2 = idDecoded.IndexOf("#", splitIndex1 + 1);
            long ticks = Convert.ToInt64(idDecoded.Substring(splitIndex1 + 1, splitIndex2 - (splitIndex1 + 1)));
            DateTime datetime = DateTime.SpecifyKind(new DateTime(ticks), DateTimeKind.Utc);
            string title = idDecoded.Substring(splitIndex2 + 1);
            return new Tuple<int, DateTime, string>(categoryId, datetime, title);
            /*
            string idDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(id));
            int splitIndex = idDecoded.IndexOf("#");
            int categoryId = Convert.ToInt32(idDecoded.Substring(0, splitIndex));
            string targetLink = idDecoded.Substring(splitIndex + 1);
            return new Tuple<int, string>(categoryId, targetLink);
            */
        }
        
        public static string EncodeExternalNewsItemId(int categoryId, DateTime datetime, string title)
        {
            string idUnencoded = categoryId + "#" + datetime.Ticks + "#" + title; // since categoryId and ticks are numbers, it is fine to use # as separator (but not fine after title).
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(idUnencoded));
            /*
            string idUnencoded = categoryId + "#" + targetUrl; // since categoryId is a number, it is fine to use # as separator (but not fine after url).
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(idUnencoded));
            */
        }

    }
}