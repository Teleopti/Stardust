using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_HintShiftBagCannotFulFillContractTime_78717)]
	[NoDefaultData]
	public class PersonContractShiftBagHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;
		
		[Test]
		public void ShouldReturnHintWhenShiftsInShiftBagAreShorterThanTargetTime()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
			};
 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(date);
			var result = Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
			result.Count().Should().Be.EqualTo(1);
			
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.ShiftsInShiftBagCanNotFulFillContractTime)), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ShiftsInShiftBagCanNotFulFillContractTime,ruleSet.Description.Name, contract.Description.Name));
		}
		
		[Test]
		public void ShouldNotReturnHintWhenShiftsInShiftBagAreWithinTolerance()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(5, 0, 0),
			};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory().WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(date);
			
			var result = Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
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
			
			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)))
				.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotReturnHintWhenAverageWorkTimeHasBeenOverridden()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
			};
 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null)
				.WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).AverageWorkTimePerDayOverride = new TimeSpan(7,0,0);

			var result = Target
				.Execute(new ScheduleHintInput(new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1),
					0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
			
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.ShiftsInShiftBagCanNotFulFillOverriddenTargetTime)), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ShiftsInShiftBagCanNotFulFillOverriddenTargetTime,ruleSet.Description.Name));
		}
		
		[Test]
		public void ShouldNotReturnHintsWhenShiftBagHasBeenDeleted()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
			};

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var shiftBag = new RuleSetBag(ruleSet).WithId();
			
			shiftBag.SetDeleted();
			
			var agent = new Person().WithId().WithPersonPeriod(shiftBag, contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null).WithSchedulePeriodOneWeek(date);
			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint))).Should().Be.Empty();
 
		}
		
		[Test]
		public void ShouldNotReturnHintsWheContractHasBeenDeleted()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
			};
			
			contract.SetDeleted();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null).WithSchedulePeriodOneWeek(date);
 
			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint))).Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotReturnHintForHourlyStaff()
		{
			var date = new DateOnly(2017, 01, 23);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				EmploymentType = EmploymentType.HourlyStaff
				
			};
	
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), contract, new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_") , null).WithSchedulePeriodOneWeek(date);
			
			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReturnHintWhenNoSchedulePeriod()
		{
			var date = new DateOnly(2017, 01, 23);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().WithPersonPeriod(new RuleSetBag(ruleSet).WithId(), new Contract("_"), new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), null);

			Target.Execute(new ScheduleHintInput(new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)))
				.Should().Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}