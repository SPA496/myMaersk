using System.Collections.Generic;
using myDamco.Database;

namespace myDamco.Models
{
    public class DashboardModel
    {        
        public Dictionary<int, Widget> Widgets; // Complete list of widgets index on id Widget name
        public List<WidgetInstance> WidgetInstances; // Complete list of widgets per for this user
        public NewsItem announcement;
    }
}