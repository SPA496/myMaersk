using System;
using myDamco.Database;

namespace myDamco.Models
{
    public class WidgetModel
    {
        public Widget Widget;
        public WidgetInstance WidgetInstance;

        public String Title {
            get {
                return !String.IsNullOrEmpty(WidgetInstance.Title) ? WidgetInstance.Title : Widget.Title;
            }
        }

        public WidgetModel (Widget widget, WidgetInstance widgetinstance) {
            Widget = widget;
            WidgetInstance = widgetinstance;
        }
    }
}