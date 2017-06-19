using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationCallbackDesktopTest : ISetup
	{
		public OptimizeIntradayIslandsDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().WithPersonPeriod(skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			}
			SchedulerStateHolderFrom.Fill(scenario, dateOnly, asses.Select(x => x.Person), asses, skillDay);

			var callbackTracker = new TrackIntradayOptimizationCallback();
			Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), callbackTracker);
			callbackTracker.SuccessfulOptimizations().Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldDoUnsuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var asses = new List<IPersonAssignment>();
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().WithPersonPeriod(skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			}
			SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, new List<ISkillDay>());

			var callbackTracker = new TrackIntradayOptimizationCallback();
			Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), callbackTracker);
			callbackTracker.UnSuccessfulOptimizations().Should().Be.EqualTo(10);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization, ICurrentIntradayOptimizationCallback>();
			system.UseTestDouble<FillSchedulerStateHolderForDesktop>().For<IFillSchedulerStateHolder>();
		}
	}
}