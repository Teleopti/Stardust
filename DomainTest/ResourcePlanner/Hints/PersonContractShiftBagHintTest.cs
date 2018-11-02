using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public class PersonContractShiftBagHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;
		
		[Test]
		public void ShouldReturnHintWhenShiftsInShiftBagAreShorterThanTargetTime()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var planningPeriod = new DateOnlyPeriod(startDate, startDate.AddDays(6));
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				PositiveDayOffTolerance = 0,
				NegativeDayOffTolerance = 0
			};
 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(startDate);
			var result = Target.Execute(new ScheduleHintInput(new[] { agent }, planningPeriod, null, false)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
			result.Count().Should().Be.EqualTo(1);
			
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.ShiftsInShiftBagCanNotFulFillContractTime)))
				.Should()
				.Be.EqualTo(string.Format(Resources.ShiftsInShiftBagCanNotFulFillContractTime,ruleSet.Description.Name, contract.Description.Name));
		}
		
		[Test]
		public void ShouldNotReturnHintWhenShiftsInShiftBagAreWithinTolerance()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var planningPeriod = new DateOnlyPeriod(startDate, startDate.AddDays(6));
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(5, 0, 0),
				PositiveDayOffTolerance = 0,
				NegativeDayOffTolerance = 0
			};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory().WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(startDate);
			
			var result = Target.Execute(new ScheduleHintInput(new[] { agent }, planningPeriod, null, false)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnHintWhenShiftLengthDiffersFromContractTime()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0)
			};
			var activityNotInContractTime = new Activity("noContractTime") {InContractTime = false}.WithId();
			var workShiftTemplateGenerator = new WorkShiftTemplateGenerator(new Activity("withContractTime").WithId(), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory().WithId());
			var ruleSet = new WorkShiftRuleSet(workShiftTemplateGenerator);
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(activityNotInContractTime,new TimePeriodWithSegment(1,0,1,0,15),new TimePeriodWithSegment(12,0,12,0,15)));
			var agent = new Person().WithId()
					.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(date);
			
			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), null, false)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)))
				.Should().Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}