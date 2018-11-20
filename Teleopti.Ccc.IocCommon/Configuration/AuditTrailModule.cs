using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Search;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.Infrastructure.Staffing;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class AuditTrailModule : Module
	{
		private readonly IocConfiguration _config;

		public AuditTrailModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PreActionAuditAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<PostActionAuditAspect>().As<IAspect>().SingleInstance();

			builder.RegisterAssemblyTypes(typeof(IHandleContextAction<>).Assembly)
				.Where(t =>
						t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleContextAction<>))
					//&& t.EnabledByToggle(_config.Toggle) may be later
				)
				.As(t => t.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IHandleContextAction<>))
				);
			builder.RegisterType<AuditAggregatorService>().SingleInstance();

			builder.RegisterType<PersonSearchProvider>().SingleInstance();
			builder.RegisterType<TenantContextReaderService>().SingleInstance();
			builder.RegisterType<StaffingContextReaderService>().As<IStaffingContextReaderService, IPurgeAudit>().AsSelf().SingleInstance();
			builder.RegisterType<PersonAccessContextReaderService>().As<IPersonAccessContextReaderService, IPurgeAudit>().AsSelf().SingleInstance();
			builder.RegisterType<PurgeAuditRunner>().AsSelf().SingleInstance();
		}
	}
}