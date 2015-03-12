using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public class RtaAreaModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public RtaAreaModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
		}
	}
}