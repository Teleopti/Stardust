using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Reporting
{
    public class ReportingAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Reporting";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
			  //context.Routes.MapPageRoute("reports",
			  // "Reporting/{controller}/{action}/{query}",
			  // "http://localhost:52858/Areas/Reporting/Index.aspx{query}"
			  // );

			  context.MapRoute(
				"Reporting_default",
				 "Reporting/{controller}/{action}/{id}",
				 new { action = "Index", id = UrlParameter.Optional }

			  );
			  

        }
    }
}