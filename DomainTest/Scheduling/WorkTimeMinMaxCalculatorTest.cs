using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class WorkTimeMinMaxCalculatorTest
	{
		[Test]
		public void ShouldReturnMinMaxWorkTimeFromRuleSetBag()
		{
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var options = MockRepository.GenerateMock<IEffectiveRestrictionOptions>();
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var scenario = MockRepository.GenerateMock<IScenario>();
			var ruleSetProjectionService = MockRepository.GenerateMock<IRuleSetProjectionService>();
			var workTimeMineMax = new WorkTimeMinMax();

			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] { personPeriod });
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			scheduleRepository.Stub(
				x =>
				x.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}), new ScheduleDictionaryLoadOptions(true, false),
				                                 new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow), scenario)).Return(
				                                 	scheduleDictionary).IgnoreArguments();
			;
			scheduleDictionary.Stub(x => x[person].ScheduledDay(DateOnly.Today)).Return(scheduleDay);
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, options)).Return(
				effectiveRestriction).IgnoreArguments();
			ruleSetBag.Stub(x => x.MinMaxWorkTime(ruleSetProjectionService, DateOnly.Today, effectiveRestriction)).Return(
				workTimeMineMax);

			var target = new WorkTimeMinMaxCalculator(ruleSetProjectionService, effectiveRestrictionForDisplayCreator,
			                                          scheduleRepository);
			var result = target.WorkTimeMinMax(person, DateOnly.Today, scenario);

			result.Should().Be.EqualTo(workTimeMineMax);
		}
	}
}
