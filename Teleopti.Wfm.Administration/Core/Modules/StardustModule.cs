using System.Configuration;
using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Core.Modules
{
	public class StardustModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public StardustModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new StaffingModule(_configuration));
			builder.Register(c => new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)).As<IStardustRepository>().SingleInstance();
			builder.RegisterType<PingNode>().As<IPingNode>().SingleInstance();
		}
	}
}