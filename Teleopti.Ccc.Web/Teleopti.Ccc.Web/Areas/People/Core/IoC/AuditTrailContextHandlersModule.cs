using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.People.Core.Aspects;

namespace Teleopti.Ccc.Web.Areas.People.Core.IoC
{
	public class AuditTrailContextHandlersModule : Module
	{
		private readonly IocConfiguration _config;

		public AuditTrailContextHandlersModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AuditTrailAspect>().As<IAspect>().SingleInstance();

			builder.RegisterAssemblyTypes(typeof(IHandleContext<>).Assembly)
				.Where(t =>
						t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleContext<>))
					//&& t.EnabledByToggle(_config.Toggle)
				)
				.As(t => t.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IHandleContext<>))
				);
		}
	}
}