using System;
using Autofac;
using MbCache.Core;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[DomainTest]
	public class RuleSetTest
	{
		public ShiftProjectionCacheFetcher ShiftProjectionCacheFetcher;
		
		[Test]
		public void VerifyProjectionServiceIsCached()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
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
		public void ShouldCacheWorkShiftWorkTime()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			containerBuilder.RegisterModule(new CommonModule(configuration));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IMbCacheFactory>()
					.IsKnownInstance(container.Resolve<IWorkShiftWorkTime>());
			}
		}

		[Test]
		public void ShouldCacheWithDifferentCallbacks()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
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
		public void ShouldNotCacheRuleSetWithNoId()
		{
			var containerBuilder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
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
		
		[Test]
		[Toggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public void ShouldCacheShiftProjectionCaches()
		{
			var wsRs = createRuleset(true);

			Assert.AreSame(ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc)), 
				ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc)));
		}
	
		[Test]
		[Toggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public void ShouldNotCacheShiftProjectionCaches_DifferentDates()
		{
			var wsRs = createRuleset(true);
			Assert.AreNotSame(ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today.AddDays(-1), TimeZoneInfo.Utc)), 
				ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc)));
		}
		
		[Test]
		[Toggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public void ShouldNotCacheShiftProjectionCaches_DifferentTimeZones()
		{
			var wsRs = createRuleset(true);
			Assert.AreNotSame(ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.AbuDhabiTimeZoneInfo())), 
				ShiftProjectionCacheFetcher.Execute(wsRs, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.AustralianTimeZoneInfo())));
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