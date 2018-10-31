using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
 
			var person = PersonFactory.CreatePerson().WithId();
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				PositiveDayOffTolerance = 0,
				NegativeDayOffTolerance = 0
			};
 
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(startDate,new PersonContract(contract, new PartTimePercentage("_"), new ContractScheduleWorkingMondayToFriday()),new Team()));
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15),	new ShiftCategory("_"))));
			person.Period(startDate).RuleSetBag = ruleSetBag;
			 
			person.AddSchedulePeriod(new SchedulePeriod(startDate,SchedulePeriodType.Week,1));
			
			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod, null, false)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
			result.Count().Should().Be.EqualTo(1);
			
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.ShiftsInShiftBagCanNotFulFillContractTime)))
				.Should()
				.Be.EqualTo(string.Format(Resources.ShiftsInShiftBagCanNotFulFillContractTime,ruleSetBag.Description.Name, contract.Description.Name));
		}
		
		[Test]
		public void ShouldNotReturnHintWhenShiftsInShiftBagAreWithinTolerance()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var planningPeriod = new DateOnlyPeriod(startDate, startDate.AddDays(6));
 
			var person = PersonFactory.CreatePerson().WithId();
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(new TimeSpan(8, 0, 0)),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				NegativePeriodWorkTimeTolerance = new TimeSpan(5, 0, 0),
				PositiveDayOffTolerance = 0,
				NegativeDayOffTolerance = 0
			};
 
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(startDate,new PersonContract(contract, new PartTimePercentage("_"), new ContractScheduleWorkingMondayToFriday()),new Team()));
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15),	new ShiftCategory("_"))));
			person.Period(startDate).RuleSetBag = ruleSetBag;
			 
			person.AddSchedulePeriod(new SchedulePeriod(startDate,SchedulePeriodType.Week,1));
			
			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod, null, false)).InvalidResources.Where(x => x.ValidationTypes.Contains(typeof(PersonContractShiftBagHint)));
 
			result.Count().Should().Be.EqualTo(0);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}