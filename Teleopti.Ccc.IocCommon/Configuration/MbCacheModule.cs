using System;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.ProxyImpl.LinFu;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MbCacheModule : Module
	{
		public ICache Cache { get; set; }

		protected override void Load(ContainerBuilder builder)
		{
			if (Cache == null)
				throw new InvalidOperationException("CacheFactory needs to be set.");
			builder.Register(c => createMbCacheFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}


		private IMbCacheFactory createMbCacheFactory()
		{
			var cacheBuilder = new CacheBuilder(new ProxyFactory(), Cache, new TeleoptiCacheKey());
			addRuleSetToCacheBuilder(cacheBuilder);
			return cacheBuilder.BuildFactory();
		}

		private static void addRuleSetToCacheBuilder(CacheBuilder cacheBuilder)
		{
			cacheBuilder
				 .For<RuleSetProjectionService>()
				 .CacheMethod(m => m.ProjectionCollection(null))
			.PerInstance()
				 .As<IRuleSetProjectionService>();
		}
	}
}