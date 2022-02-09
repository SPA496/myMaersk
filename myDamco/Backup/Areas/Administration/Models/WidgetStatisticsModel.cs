using System.Collections.Generic;
using myDamco.Database;

namespace myDamco.Areas.Administration.Models
{
    public class WidgetStatisticsModel
    {
        // Widget usage statistics
        public class WidgetStat
        {
            public Widget widget;
            public int count;
        }

        public IList<WidgetStat> widgetStats = new List<WidgetStat>();
        public string from;
        public string to;
    }
}