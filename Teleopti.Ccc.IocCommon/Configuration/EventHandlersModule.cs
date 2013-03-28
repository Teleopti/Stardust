using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class EventHandlersModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(IHandleEvent<>).Assembly)
			       .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)))
			       .As(t => t.GetInterfaces().Single(i => i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)))
				;
		}
	}
}