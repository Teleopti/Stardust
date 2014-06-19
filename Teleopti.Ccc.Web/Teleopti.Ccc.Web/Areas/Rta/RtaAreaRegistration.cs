using System.ServiceModel.Activation;
using System.Web.Mvc;
using Autofac.Integration.Wcf;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public class RtaAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Rta";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Rta-default",
				"Rta/{controller}/{action}",
				new { controller = "Service", action = "SaveExternalUserState" });

			context.Routes.Add(new ServiceRoute("Rta/TeleoptiRtaService.svc", new AutofacServiceHostFactory(), typeof(TeleoptiRtaService)));
			context.Routes.Add(new ServiceRoute("TeleoptiRtaService.svc", new AutofacServiceHostFactory(), typeof(TeleoptiRtaService)));
		}
	}
}
