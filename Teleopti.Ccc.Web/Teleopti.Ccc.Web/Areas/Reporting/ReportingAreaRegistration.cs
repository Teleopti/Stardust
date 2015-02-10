using System.Web.Mvc;
using Microsoft.ReportingServices.Interfaces;

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
			  context.MapRoute(
				"Reporting_default",
				 "Reporting/{controller}/{id}",
				 new { Controller = "Report", action = "Index", id = UrlParameter.Optional }

			  );

			  context.MapRoute(
				"Reporting_index",
				 "Reporting/{controller}/{action}/{id}",
				 new { Controller = "Report", action = "Index", id = UrlParameter.Optional }

			  );
			  

        }
    }
}