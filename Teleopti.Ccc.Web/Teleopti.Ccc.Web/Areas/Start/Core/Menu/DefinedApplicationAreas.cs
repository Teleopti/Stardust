namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	using System.Collections.Generic;

	using Teleopti.Ccc.Domain.Security.AuthorizationData;
	using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

	public static class DefinedApplicationAreas
	{
		private static readonly List<ApplicationArea> ApplicationApplicationAreas = new List<ApplicationArea> { new ApplicationArea { ApplicationFunctionPath = DefinedRaptorApplicationFunctionPaths.Anywhere, Area = "MobileReports", Name = "Anywhere" }, new ApplicationArea { ApplicationFunctionPath = DefinedRaptorApplicationFunctionPaths.MyTimeWeb, Area = "MyTime", Name = "MyTime" } };

		public static IEnumerable<ApplicationArea> ApplicationAreas
		{
			get
			{
				return ApplicationApplicationAreas;
			}
		}
	}
}