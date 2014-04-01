using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Diagnosis
{
	public class DiagnosisAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Diagnosis";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
			"Diagnosis-authentication",
			"Diagnosis/Authentication/{action}",
			new { controller = "Authentication", action = "SignIn", area = "Start", origin = "Diagnosis" },
			null,
			new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"Diagnosis_default",
				"Diagnosis/{controller}/{action}/{id}",
				new { controller = "Application" ,action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
