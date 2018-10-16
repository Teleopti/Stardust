using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;

namespace Teleopti.Ccc.IocCommon.Configuration
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

			builder.RegisterAssemblyTypes(typeof(IHandleContextAction<>).Assembly)
				.Where(t =>
						t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleContextAction<>))
					//&& t.EnabledByToggle(_config.Toggle)
				)
				.As(t => t.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IHandleContextAction<>))
				);
			builder.RegisterType<AuditAggregatorService>().SingleInstance();

			////may be we should move it to domain or its own module or a global module
			//if (_config.Toggle(Toggles.Wfm_AuditTrail_StaffingAuditTrail_78125))
			//	builder.RegisterType<AuditableBpoOperationsToggleOn>().As<IAuditableBpoOperations>().SingleInstance().ApplyAspects();
			//else
			//	builder.RegisterType<AuditableBpoOperationsToggleOff>().As<IAuditableBpoOperations>().SingleInstance();

		}
	}
}