﻿using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetModule : Module
	{
		public bool CacheRuleSetProjection { get; set; }
		public bool RegisterRuleSetProjectionService { get; set; }

		public RuleSetModule() { RegisterRuleSetProjectionService = true; }

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>()
				 .As<ICreateWorkShiftsFromTemplate>()
				 .SingleInstance();
			builder.RegisterType<ShiftCreatorService>()
				 .As<IShiftCreatorService>()
				 .SingleInstance();

			if (RegisterRuleSetProjectionService)
			{
				if (CacheRuleSetProjection)
				{
					builder.Register(componentContext =>
									 componentContext.Resolve<IMbCacheFactory>()
										.Create<IRuleSetProjectionService>(componentContext.Resolve<IShiftCreatorService>())
						)
						.OnRelease(inValidateCache)
						.As<IRuleSetProjectionService>()
						.InstancePerLifetimeScope();
				}
				else
				{
					builder.RegisterType<RuleSetProjectionService>()
						.As<IRuleSetProjectionService>()
						.InstancePerLifetimeScope();
				}
			}
		}

		private static void inValidateCache(IRuleSetProjectionService obj)
		{
			((ICachingComponent)obj).Invalidate();
		}
	}
}