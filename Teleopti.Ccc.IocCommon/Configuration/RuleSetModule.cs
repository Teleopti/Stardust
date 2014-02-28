using System;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetModule : Module
	{
		private readonly bool _perLifetimeScope;
		private readonly CacheBuilder _cacheBuilder;

		public RuleSetModule(MbCacheModule mbCacheModule, bool perLifetimeScope) 
		{
			_perLifetimeScope = perLifetimeScope;
			if (mbCacheModule == null)
				throw new ArgumentException("MbCacheModule required", "mbCacheModule");
			_cacheBuilder = mbCacheModule.Builder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>()
				.As<ICreateWorkShiftsFromTemplate>()
				.SingleInstance();
			builder.RegisterType<ShiftCreatorService>()
				.As<IShiftCreatorService>()
				.SingleInstance();
			builder.RegisterType<RuleSetProjectionService>().AsSelf();
			builder.RegisterType<RuleSetProjectionEntityService>().AsSelf();
			builder.RegisterType<WorkShiftWorkTime>().AsSelf();

			if (_perLifetimeScope)
			{
				builder.Register<IRuleSetProjectionService>(c => c.Resolve<RuleSetProjectionService>())
					.InstancePerLifetimeScope()
					.IntegrateWithMbCache()
					.OnRelease(s => ((ICachingComponent)s).Invalidate());
				builder.Register<IRuleSetProjectionEntityService>(c => c.Resolve<RuleSetProjectionEntityService>())
					.InstancePerLifetimeScope()
					.IntegrateWithMbCache()
					.OnRelease(s => ((ICachingComponent) s).Invalidate());
				builder.Register<IWorkShiftWorkTime>(c => c.Resolve<WorkShiftWorkTime>())
					.IntegrateWithMbCache()
					.OnRelease(s => ((ICachingComponent)s).Invalidate());
			}
			else
			{
				builder.Register<IRuleSetProjectionService>(c => c.Resolve<RuleSetProjectionService>())
					.IntegrateWithMbCache<IRuleSetProjectionService>()
					.SingleInstance();
				builder.Register<IRuleSetProjectionEntityService>(c => c.Resolve<RuleSetProjectionEntityService>())
					.IntegrateWithMbCache<IRuleSetProjectionEntityService>()
					.SingleInstance();
				builder.Register<IWorkShiftWorkTime>(c => c.Resolve<WorkShiftWorkTime>())
					.IntegrateWithMbCache<IWorkShiftWorkTime>()
					.SingleInstance();
			}
			_cacheBuilder
				.For<RuleSetProjectionService>("RSPS")
					.CacheMethod(m => m.ProjectionCollection(null,null))
					.PerInstance()
					.As<IRuleSetProjectionService>()
				.For<RuleSetProjectionEntityService>("RSPES")
					.CacheMethod(m => m.ProjectionCollection(null, null))
					.PerInstance()
					.As<IRuleSetProjectionEntityService>()
				.For<WorkShiftWorkTime>("WSWT")
					.CacheMethod(m => m.CalculateMinMax(null, null))
					.PerInstance()
					.As<IWorkShiftWorkTime>();
		}
	}
}