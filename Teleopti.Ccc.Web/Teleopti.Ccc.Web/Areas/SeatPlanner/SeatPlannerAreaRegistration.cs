using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner
{
	public class SeatPlannerAreaRegistration : AreaRegistration
	{

		public override string AreaName
		{
            get { return "SeatPlanner"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"SeatPlanner-authentication",
				"SeatPlanner/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "SeatPlanner" },
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"SeatPlanner-default",
				"SeatPlanner/{controller}/{action}",
				new { controller = "Application", action = "Index" }
				);
		}

	}
}