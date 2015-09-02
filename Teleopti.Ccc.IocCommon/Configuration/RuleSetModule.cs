using System;
using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetModule : Module
	{
		private readonly bool _perLifetimeScope;
		private readonly IIocConfiguration _configuration;

		public RuleSetModule(IIocConfiguration configuration, bool perLifetimeScope) 
		{
			_perLifetimeScope = perLifetimeScope;
			if (configuration == null)
				throw new ArgumentException("MbCacheModule required", "configuration");
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>().As<ICreateWorkShiftsFromTemplate>().SingleInstance();
			builder.RegisterType<ShiftCreatorService>().As<IShiftCreatorService>().SingleInstance();
			if (_perLifetimeScope)
			{
				builder.RegisterMbCacheComponent<RuleSetProjectionService, IRuleSetProjectionService>()
					.InstancePerLifetimeScope()
					.OnRelease(s => ((ICachingComponent)s).Invalidate());
				builder.RegisterMbCacheComponent<RuleSetProjectionEntityService, IRuleSetProjectionEntityService>()
					.InstancePerLifetimeScope()
					.OnRelease(s => ((ICachingComponent) s).Invalidate());
				builder.RegisterMbCacheComponent<WorkShiftWorkTime, IWorkShiftWorkTime>()
					.OnRelease(s => ((ICachingComponent)s).Invalidate());
			}
			else
			{
				builder.RegisterMbCacheComponent<RuleSetProjectionService, IRuleSetProjectionService>()
					.SingleInstance();
				builder.RegisterMbCacheComponent<RuleSetProjectionEntityService, IRuleSetProjectionEntityService>()
					.SingleInstance();
				builder.RegisterMbCacheComponent<WorkShiftWorkTime, IWorkShiftWorkTime>()
					.SingleInstance();
			}
			_configuration.Args().Cache(b => b
				.For<RuleSetProjectionService>("RSPS")
				.CacheMethod(m => m.ProjectionCollection(null, null))
				.PerInstance()
				.As<IRuleSetProjectionService>()
				.For<RuleSetProjectionEntityService>("RSPES")
				.CacheMethod(m => m.ProjectionCollection(null, null))
				.PerInstance()
				.As<IRuleSetProjectionEntityService>()
				.For<WorkShiftWorkTime>("WSWT")
				.CacheMethod(m => m.CalculateMinMax(null, null))
				.PerInstance()
				.As<IWorkShiftWorkTime>()
				);
		}
	}
}