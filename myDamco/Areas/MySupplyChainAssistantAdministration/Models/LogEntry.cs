namespace myDamco.Areas.MySupplyChainAssistantAdministration.Models
{
    public class LogEntry
    {
        public string function { get; set; }
        public string hook { get; set; }
        public string data { get; set; }
        public User user { get; set; }

        public class User
        {
            public string name { get; set; }
            public string role { get; set; }
        }
    }
}