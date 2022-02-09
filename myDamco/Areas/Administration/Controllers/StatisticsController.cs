using System;
using System.Linq;
using System.Web.Mvc;
using myDamco.Areas.Administration.Models;
using myDamco.Database;
using myDamco.Utils;

namespace myDamco.Areas.Administration.Controllers
{
    public class StatisticsController : BaseAdminController
    {
        //
        // GET: /Administration/Statistics/

        private myDamcoEntities db = new myDamcoEntities();

        private const string LOCALTIME_FORMAT = "yyyy-MM-dd HH:mm:ss \"UTC\"zzz";
        private const string UTCTIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private const string UTCTIME_NOSECONDS_FORMAT = "yyyy-MM-dd HH:mm";


        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Index()
        {
            DateTime dbUtcTime = db.Database.SqlQuery<DateTime>("select SysUtcDateTime()").Single();
            DateTimeOffset dbLocalTime = db.Database.SqlQuery<DateTimeOffset>("select SysDateTimeOffset()").Single();

            var model = new StatisticsModel();
            model.webserverTimeUtc   = DateTime.UtcNow.ToString(UTCTIME_FORMAT);
            model.webserverTimeLocal = DateTime.Now.ToString(LOCALTIME_FORMAT);
            model.dbTimeUtc          = dbUtcTime.ToString(UTCTIME_FORMAT);
            model.dbTimeLocal        = dbLocalTime.ToString(LOCALTIME_FORMAT);
            model.buildTimeUtc       = ControllerUtil.GetBuildTimeUtc().ToString(UTCTIME_FORMAT);
            return View(model);
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public PartialViewResult WidgetStatistics(DateTime from, DateTime to)
        {
            // Remove seconds and miliseconds from the DateTime objects
            from = from.AddTicks(-(from.Ticks % TimeSpan.TicksPerMinute));
            to = to.AddTicks(-(to.Ticks % TimeSpan.TicksPerMinute));

            // Mark the DateTime objects as UTC
            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var filteredHistoryRows =
                from h in db.WidgetInstanceHistory
                where (h.DeleteTime >= @from || h.DeleteTime == null) && h.AddTime <= @to
                select h;

            var leftGroupJoin = // (the left join is in order to also get an entry for widgets not on any dashboard in the given time-period) (info on groupjoin: http://stackoverflow.com/questions/15595289/linq-to-entities-join-vs-groupjoin )
                from w in db.Widget
                join h in filteredHistoryRows on w.Id equals h.Widget_Id into groupJoined
                select new WidgetStatisticsModel.WidgetStat() { widget = w, count = groupJoined.Count() };

            var widgetStats = leftGroupJoin.ToList();

            var model = new WidgetStatisticsModel()
            {
                widgetStats = widgetStats,
                from = from.ToString(UTCTIME_NOSECONDS_FORMAT),
                to = to.ToString(UTCTIME_NOSECONDS_FORMAT)
            };

            return PartialView("_WidgetStatistics", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (db != null)
                db.Dispose();
            base.Dispose(disposing);
        }

    }
}
