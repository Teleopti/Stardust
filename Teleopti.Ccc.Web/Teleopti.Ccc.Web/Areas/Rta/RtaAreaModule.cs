using Autofac;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public class RtaAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
			builder.RegisterType<PermissionsViewModelBuilder>().AsSelf().SingleInstance();
		}
	}
}