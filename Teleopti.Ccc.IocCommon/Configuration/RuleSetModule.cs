using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetModule : Module
	{
		public bool CacheRuleSetProjection { get; set; }
		public bool SingleInstanceRuleSetProjectionService { get; set; }

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>()
				 .As<ICreateWorkShiftsFromTemplate>()
				 .SingleInstance();
			builder.RegisterType<ShiftCreatorService>()
				 .As<IShiftCreatorService>()
				 .SingleInstance();
			if (CacheRuleSetProjection)
			{
				var ruleSetProjectionService = builder.Register(componentContext =>
				                              componentContext.Resolve<IMbCacheFactory>()
				                              	.Create<IRuleSetProjectionService>(componentContext.Resolve<IShiftCreatorService>())
					)
					.OnRelease(inValidateCache)
					.As<IRuleSetProjectionService>();
				if (SingleInstanceRuleSetProjectionService)
					ruleSetProjectionService.SingleInstance();
				else
					ruleSetProjectionService.InstancePerLifetimeScope();
			}
			else
			{
				var ruleSetProjectionService = builder.RegisterType<RuleSetProjectionService>()
					.As<IRuleSetProjectionService>();
				if (SingleInstanceRuleSetProjectionService)
					ruleSetProjectionService.SingleInstance();
				else
					ruleSetProjectionService.InstancePerLifetimeScope();
			}
		}

		private static void inValidateCache(IRuleSetProjectionService obj)
		{
			((ICachingComponent)obj).Invalidate();
		}
	}
}