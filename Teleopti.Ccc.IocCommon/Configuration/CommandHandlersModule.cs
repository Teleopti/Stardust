using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class CommandHandlersModule : Module
	{
		private readonly IIocConfiguration _config;

		public CommandHandlersModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(IHandleCommand<>).Assembly)
			       .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleCommand<>)) 
				   && t.EnabledByToggle(_config))
			       .As(t => t.GetInterfaces().Single(i => i.GetGenericTypeDefinition() == typeof (IHandleCommand<>)));
		}
	}
}