using System;
using System.Collections.Generic;
using MbCache.Core;
using NUnit.Framework;
using Autofac;
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
			var mbCacheModule = new MbCacheModule(null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetCacheModule(mbCacheModule, true));
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);

				var projSvc = container.Resolve<IRuleSetProjectionEntityService>();
				var projSvc2 = container.Resolve<IRuleSetProjectionEntityService>();

				Assert.AreSame(projSvc.ProjectionCollection(wsRs), projSvc2.ProjectionCollection(wsRs));

			}
		}

		[Test]
		public void ProjectionServiceIsCachedPerScope()
		{
			var mbCacheModule = new MbCacheModule(null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetCacheModule(mbCacheModule, true));
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

				Assert.AreNotSame(projSvc.ProjectionCollection(wsRs), projSvc2.ProjectionCollection(wsRs));
			}
		}

		[Test]
		public void ShouldCacheWorkShiftWorkTime()
		{
			var mbCacheModule = new MbCacheModule(null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetCacheModule(mbCacheModule, true));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IMbCacheFactory>()
					.IsKnownInstance(container.Resolve<IWorkShiftWorkTime>());
			}
		}

		[Test]
		public void CacheShouldBeInvalidatedWhenContainerScopeIsDead()
		{
			var mbCacheModule = new MbCacheModule(null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetCacheModule(mbCacheModule, true));
			var wsRs = createRuleset(true);

			using (var container = containerBuilder.Build())
			{
				IEnumerable<IWorkShiftVisualLayerInfo> proj;
				IRuleSetProjectionEntityService projSvc;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
					proj = projSvc.ProjectionCollection(wsRs);

				}
				Assert.AreNotSame(proj, projSvc.ProjectionCollection(wsRs));
			}
		}

		[Test]
		public void ShouldNotCacheRuleSetWithNoId()
		{
			var mbCacheModule = new MbCacheModule(null);
			containerBuilder.RegisterModule(mbCacheModule);
			containerBuilder.RegisterModule(new RuleSetCacheModule(mbCacheModule, true));
			var wsRs = createRuleset(false);

			using (var container = containerBuilder.Build())
			{
				using (var inner1 = container.BeginLifetimeScope())
				{
					var projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
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
	}
}