using System;
using System.Collections.Generic;
using Autofac;
using MbCache.Core;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class RuleSetTest
	{
		[Test]
		public void VerifyProjectionServiceIsCached([Values(true, false)] bool perLifeTimeScope)
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			configuration.Args().CacheRulesetPerLifeTimeScope = perLifeTimeScope;
			containerBuilder.RegisterModule(new CommonModule(configuration));
			var callback = new WorkShiftAddStopperCallback();
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);

				var projSvc = container.Resolve<IRuleSetProjectionEntityService>();
				var projSvc2 = container.Resolve<IRuleSetProjectionEntityService>();

				Assert.AreSame(projSvc.ProjectionCollection(wsRs, callback), projSvc2.ProjectionCollection(wsRs, callback));
			}
		}

		[Test]
		public void ProjectionServiceIsCachedPerScope()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			containerBuilder.RegisterModule(new CommonModule(configuration));
			var wsRs = createRuleset(true);
			var callback = new WorkShiftAddStopperCallback();
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

				Assert.AreNotSame(projSvc.ProjectionCollection(wsRs, callback), projSvc2.ProjectionCollection(wsRs, callback));
			}
		}

		[Test]
		public void ShouldCacheWorkShiftWorkTime([Values(true, false)] bool perLifeTimeScope)
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			configuration.Args().CacheRulesetPerLifeTimeScope = perLifeTimeScope;
			containerBuilder.RegisterModule(new CommonModule(configuration));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IMbCacheFactory>()
					.IsKnownInstance(container.Resolve<IWorkShiftWorkTime>());
			}
		}

		[Test]
		public void CacheShouldBeInvalidatedWhenContainerScopeIsDead()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			containerBuilder.RegisterModule(new CommonModule(configuration));
			var wsRs = createRuleset(true);
			var callback = new WorkShiftAddStopperCallback();
			using (var container = containerBuilder.Build())
			{
				IEnumerable<WorkShiftVisualLayerInfo> proj;
				IRuleSetProjectionEntityService projSvc;
				using (var inner1 = container.BeginLifetimeScope())
				{
					projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
					proj = projSvc.ProjectionCollection(wsRs, callback);

				}
				Assert.AreNotSame(proj, projSvc.ProjectionCollection(wsRs, callback));
			}
		}

		[Test]
		public void ShouldCacheWithDifferentCallbacks([Values(true, false)] bool perLifeTimeScope)
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			configuration.Args().CacheRulesetPerLifeTimeScope = perLifeTimeScope;
			containerBuilder.RegisterModule(new CommonModule(configuration));
			using (var container = containerBuilder.Build())
			{
				var wsRs = createRuleset(true);

				var projSvc = container.Resolve<IRuleSetProjectionEntityService>();
				var projSvc2 = container.Resolve<IRuleSetProjectionEntityService>();

				Assert.AreSame(projSvc.ProjectionCollection(wsRs, new WorkShiftAddStopperCallback()), projSvc2.ProjectionCollection(wsRs, new WorkShiftAddStopperCallback()));
			}
		}

		[Test]
		public void ShouldNotCacheRuleSetWithNoId([Values(true, false)] bool perLifeTimeScope)
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			configuration.Args().CacheRulesetPerLifeTimeScope = perLifeTimeScope;
			containerBuilder.RegisterModule(new CommonModule(configuration));
			var wsRs = createRuleset(false);
			var callback = new WorkShiftAddStopperCallback();
			using (var container = containerBuilder.Build())
			{
				using (var inner1 = container.BeginLifetimeScope())
				{
					var projSvc = inner1.Resolve<IRuleSetProjectionEntityService>();
					Assert.AreNotSame(projSvc.ProjectionCollection(wsRs, callback), projSvc.ProjectionCollection(wsRs, callback));
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