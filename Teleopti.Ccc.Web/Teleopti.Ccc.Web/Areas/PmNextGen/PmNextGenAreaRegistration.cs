using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.PmNextGen
{
    public class PmNextGenAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PmNextGen";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PmNextGen_default",
                "PmNextGen/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}