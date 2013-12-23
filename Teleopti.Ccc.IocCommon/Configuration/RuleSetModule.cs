using System;
using Autofac;
using Autofac.Builder;
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

			if (_perLifetimeScope)
			{
				builder
					.RegisterRuleSetProjectionService()
					.OnRelease(s => ((ICachingComponent) s).Invalidate())
					.InstancePerLifetimeScope();
				builder
					.RegisterRuleSetProjectionEntityService()
					.OnRelease(s => ((ICachingComponent) s).Invalidate())
					.InstancePerLifetimeScope();
				builder
					.RegisterWorkShiftWorkTime()
					.OnRelease(s => ((ICachingComponent) s).Invalidate());
			}
			else
			{
				builder
					.RegisterRuleSetProjectionService()
					.SingleInstance();
				builder
					.RegisterRuleSetProjectionEntityService()
					.SingleInstance();
				builder
					.RegisterWorkShiftWorkTime()
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

		public static IRegistrationBuilder<IWorkShiftWorkTime, SimpleActivatorData, SingleRegistrationStyle> RegisterWorkShiftWorkTime(this ContainerBuilder builder)
		{
			return builder.Register(c =>
			                        	{
			                        		var ruleSetProjectionService = c.Resolve<IRuleSetProjectionService>();
			                        		var cacheProxyFactory = c.Resolve<IMbCacheFactory>();
			                        		var instance = cacheProxyFactory.Create<IWorkShiftWorkTime>(ruleSetProjectionService);
			                        		return instance;
			                        	}
				).As<IWorkShiftWorkTime>();

		}
	}


}