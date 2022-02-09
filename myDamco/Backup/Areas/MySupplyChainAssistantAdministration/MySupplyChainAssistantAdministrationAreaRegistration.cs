using System.Web.Mvc;

namespace myDamco.Areas.MySupplyChainAssistantAdministration
{
    public class MySupplyChainAssistantAdministrationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MySupplyChainAssistantAdministration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "MySupplyChainAssistantAdministration_default",
                "MySupplyChainAssistantAdministration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
