using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class CommandHandlersModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof (AddFullDayAbsenceCommandHandler).Assembly)
			       .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleCommand<>)))
			       .As(t => t.GetInterfaces().Single(i => i.GetGenericTypeDefinition() == typeof (IHandleCommand<>)))
				;
		}
	}
}