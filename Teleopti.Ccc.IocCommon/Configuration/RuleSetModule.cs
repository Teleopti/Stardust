using System;
using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RuleSetModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public RuleSetModule(IIocConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentException("MbCacheModule required", nameof(configuration));
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>().As<ICreateWorkShiftsFromTemplate>().SingleInstance();
			builder.RegisterType<ShiftCreatorService>().As<IShiftCreatorService>().SingleInstance();
			if (_configuration.Args().CacheRulesetPerLifeTimeScope)
			{
				builder.CacheByInterfaceProxy<RuleSetProjectionService, IRuleSetProjectionService>()
					.InstancePerLifetimeScope()
					.OnRelease(s => ((ICachingComponent)s).Invalidate());

				builder.CacheByInterfaceProxy<RuleSetProjectionEntityService, IRuleSetProjectionEntityService>()
					.InstancePerLifetimeScope()
					.OnRelease(s => ((ICachingComponent) s).Invalidate());

				builder.CacheByInterfaceProxy<WorkShiftWorkTime, IWorkShiftWorkTime>()
					.InstancePerLifetimeScope()
					.OnRelease(s => ((ICachingComponent) s).Invalidate());
			}
			else
			{
				builder.CacheByInterfaceProxy<RuleSetProjectionService, IRuleSetProjectionService>().SingleInstance();
				builder.CacheByInterfaceProxy<RuleSetProjectionEntityService, IRuleSetProjectionEntityService>().SingleInstance();
				builder.CacheByInterfaceProxy<WorkShiftWorkTime, IWorkShiftWorkTime>().SingleInstance();
			}

			_configuration.Cache().This<IRuleSetProjectionService>(b => b
				.CacheMethod(m => m.ProjectionCollection(null, null))
				.PerInstance()
				, "RSPS");

			_configuration.Cache().This<IRuleSetProjectionEntityService>(b => b
				.CacheMethod(m => m.ProjectionCollection(null, null))
				.PerInstance()
				, "RSPES");

			_configuration.Cache().This<IWorkShiftWorkTime>(b => b
				.CacheMethod(m => m.CalculateMinMax(null, null))
				.PerInstance()
				, "WSWT");
		}
	}
}