using System;
using System.Collections.Generic;
using MbCache.Configuration;
using NUnit.Framework;
using Autofac;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class RuleSetTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
		}

		[Test]
		public void VerifyProjectionServiceIsCached()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = true });
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);

				var projSvc = container.Resolve<IRuleSetProjectionService>();
				var projSvc2 = container.Resolve<IRuleSetProjectionService>();

				Assert.AreSame(projSvc.ProjectionCollection(wsRs), projSvc2.ProjectionCollection(wsRs));

			}
		}

		[Test]
		public void ProjectionServiceIsCachedPerScope()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = true });
			var wsRs = createRuleset(true);

			using (var container = containerBuilder.Build())
			{
				IRuleSetProjectionService projSvc;
				IRuleSetProjectionService projSvc2;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionService>();
				}
				using (var inner2 = container.BeginLifetimeScope())
				{
					projSvc2 = inner2.Resolve<IRuleSetProjectionService>();
				}

				Assert.AreNotSame(projSvc.ProjectionCollection(wsRs), projSvc2.ProjectionCollection(wsRs));
			}
		}

		[Test]
		public void CacheShouldBeInvalidatedWhenContainerScopeIsDead()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = true });
			var wsRs = createRuleset(true);

			using (var container = containerBuilder.Build())
			{
				IEnumerable<IWorkShiftVisualLayerInfo> proj;
				IRuleSetProjectionService projSvc;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionService>();
					proj = projSvc.ProjectionCollection(wsRs);

				}
				Assert.AreNotSame(proj, projSvc.ProjectionCollection(wsRs));
			}
		}

		[Test]
		public void VerifyProjectionServiceIsNotCached()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = false });
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);
				var projSvc = container.Resolve<IRuleSetProjectionService>();

				Assert.AreNotSame(projSvc.ProjectionCollection(wsRs), projSvc.ProjectionCollection(wsRs));
			}
		}

		[Test]
		public void ShouldNotCacheRuleSetWithNoId()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = true });
			var wsRs = createRuleset(false);

			using (var container = containerBuilder.Build())
			{
				using (var inner1 = container.BeginLifetimeScope())
				{
					var projSvc = inner1.Resolve<IRuleSetProjectionService>();
					Assert.AreNotSame(projSvc.ProjectionCollection(wsRs), projSvc.ProjectionCollection(wsRs));
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

		[Test]
		public void ShouldRegisterWorkTimeMinMaxCalculator()
		{
			containerBuilder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			containerBuilder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = true });
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IWorkTimeMinMaxCalculator>()
					.Should().Not.Be.Null();
			}
		}
	}
}