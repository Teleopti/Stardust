using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.IoC
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
			builder.RegisterModule(new RtaModule(_configuration));
		}
	}
}