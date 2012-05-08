using System;
using Autofac;
using Autofac.Builder;
using MbCache.Configuration;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetCacheModule : Module
	{
		private readonly bool _perLifetimeScope;
		private readonly CacheBuilder _cacheBuilder;

		public RuleSetCacheModule(MbCacheModule mbCacheModule, bool perLifetimeScope) {
			_perLifetimeScope = perLifetimeScope;
			if (mbCacheModule == null)
				throw new ArgumentException("MbCacheModule required", "mbCacheModule");
			_cacheBuilder = mbCacheModule.Builder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (_perLifetimeScope)
			{
				builder
					.RegisterRuleSetProjectionService()
				.OnRelease(s => ((ICachingComponent) s).Invalidate())
				.InstancePerLifetimeScope()
				;
			}
			else
			{
				builder
					.RegisterRuleSetProjectionService()
					.SingleInstance()
					;
			}
			_cacheBuilder
				.For<RuleSetProjectionService>()
				.CacheMethod(m => m.ProjectionCollection(null))
				.PerInstance()
				.As<IRuleSetProjectionService>()
				;



			if (_perLifetimeScope)
			{
				builder
					.RegisterRuleSetProjectionEntityService()
					.OnRelease(s => ((ICachingComponent)s).Invalidate())
					.InstancePerLifetimeScope()
					;
			}
			else
			{
				builder
					.RegisterRuleSetProjectionEntityService()
					.SingleInstance()
					;
			}
			_cacheBuilder
				.For<RuleSetProjectionEntityService>()
				.CacheMethod(m => m.ProjectionCollection(null))
				.PerInstance()
				.As<IRuleSetProjectionEntityService>()
				;

		}
	}

	public static class Extensions
	{
		public static IRegistrationBuilder<IRuleSetProjectionService, SimpleActivatorData, SingleRegistrationStyle> RegisterRuleSetProjectionService(this ContainerBuilder builder)
		{
			return builder.Register(c =>
			                        	{
			                        		var shiftCreatorService = c.Resolve<IShiftCreatorService>();
			                        		var cacheProxyFactory = c.Resolve<IMbCacheFactory>();
			                        		var instance = cacheProxyFactory.Create<IRuleSetProjectionService>(shiftCreatorService);
			                        		return instance;
			                        	})
				.As<IRuleSetProjectionService>()
				;
		}


		public static IRegistrationBuilder<IRuleSetProjectionEntityService, SimpleActivatorData, SingleRegistrationStyle> RegisterRuleSetProjectionEntityService(this ContainerBuilder builder)
		{
			return builder.Register(c =>
			                        	{
			                        		var shiftCreatorService = c.Resolve<IShiftCreatorService>();
			                        		var cacheProxyFactory = c.Resolve<IMbCacheFactory>();
			                        		var instance = cacheProxyFactory.Create<IRuleSetProjectionEntityService>(shiftCreatorService);
			                        		return instance;
			                        	}
				)
				.As<IRuleSetProjectionEntityService>();
		}
	}


}