using System;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetCacheModule : Module
	{
		private readonly CacheBuilder _cacheBuilder;

		public RuleSetCacheModule(MbCacheModule mbCacheModule) {
			if (mbCacheModule == null)
				throw new ArgumentException();
			_cacheBuilder = mbCacheModule.Builder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c =>
			                 c.Resolve<IMbCacheFactory>()
			                 	.Create<IRuleSetProjectionService>(c.Resolve<IShiftCreatorService>())
				)
				.OnRelease(s => ((ICachingComponent) s).Invalidate())
				.As<IRuleSetProjectionService>()
				.InstancePerLifetimeScope()
				;

			_cacheBuilder
				.For<RuleSetProjectionService>()
				.CacheMethod(m => m.ProjectionCollection(null))
				.PerInstance()
				.As<IRuleSetProjectionService>()
				;

		}
	}
}