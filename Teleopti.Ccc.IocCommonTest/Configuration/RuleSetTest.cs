using System;
using System.Collections.Generic;
using MbCache.Configuration;
using MbCache.Core;
using NUnit.Framework;
using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class RuleSetTest
	{
		private ContainerBuilder containerBuilder;
		private IWorkShiftAddCallback _callback;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			_callback = new WorkShiftAddStopperCallback();
		}

		[Test]
		public void VerifyProjectionServiceIsCached([Values(true, false)] bool perLifeTimeScope)
		{
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetModule(mbCacheModule, perLifeTimeScope));
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);

				var projSvc = container.Resolve<IRuleSetProjectionEntityService>();
				var projSvc2 = container.Resolve<IRuleSetProjectionEntityService>();

				Assert.AreSame(projSvc.ProjectionCollection(wsRs, _callback), projSvc2.ProjectionCollection(wsRs, _callback));

			}
		}

		[Test]
		public void ProjectionServiceIsCachedPerScope()
		{
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetModule(mbCacheModule, true));
			var wsRs = createRuleset(true);

			using (var container = containerBuilder.Build())
			{
				IRuleSetProjectionEntityService projSvc;
				IRuleSetProjectionEntityService projSvc2;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
				}
				using (var inner2 = container.BeginLifetimeScope())
				{
					projSvc2 = inner2.Resolve<IRuleSetProjectionEntityService>();
				}

				Assert.AreNotSame(projSvc.ProjectionCollection(wsRs, _callback), projSvc2.ProjectionCollection(wsRs, _callback));
			}
		}

		[Test]
		public void ShouldCacheWorkShiftWorkTime([Values(true, false)] bool perLifeTimeScope)
		{
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetModule(mbCacheModule, perLifeTimeScope));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IMbCacheFactory>()
					.IsKnownInstance(container.Resolve<IWorkShiftWorkTime>());
			}
		}

		[Test]
		public void CacheShouldBeInvalidatedWhenContainerScopeIsDead()
		{
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetModule(mbCacheModule, true));
			var wsRs = createRuleset(true);

			using (var container = containerBuilder.Build())
			{
				IEnumerable<IWorkShiftVisualLayerInfo> proj;
				IRuleSetProjectionEntityService projSvc;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
					proj = projSvc.ProjectionCollection(wsRs, _callback);

				}
				Assert.AreNotSame(proj, projSvc.ProjectionCollection(wsRs, _callback));
			}
		}

		[Test]
		public void ShouldNotCacheRuleSetWithNoId([Values(true, false)] bool perLifeTimeScope)
		{
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetModule(mbCacheModule, perLifeTimeScope));
			var wsRs = createRuleset(false);

			using (var container = containerBuilder.Build())
			{
				using (var inner1 = container.BeginLifetimeScope())
				{
					var projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
					Assert.AreNotSame(projSvc.ProjectionCollection(wsRs, _callback), projSvc.ProjectionCollection(wsRs, _callback));
				}
			}
		}

		private static IWorkShiftRuleSet createRuleset(bool withId)
		{
			var wsRs = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("f"),
										new TimePeriodWithSegment(10, 10, 11, 10, 10),
										new TimePeriodWithSegment(10, 10, 11, 10, 10),
										new ShiftCategory("sdf")));
			if(withId)
				wsRs.SetId(Guid.NewGuid());
			return wsRs;
		}
	}
}