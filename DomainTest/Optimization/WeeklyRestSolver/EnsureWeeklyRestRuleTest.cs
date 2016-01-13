using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class EnsureWeeklyRestRuleTest
    {
        private IEnsureWeeklyRestRule _target;
        private IPerson _person;
	    private FakeScheduleRepository _scheduleRepository;

	    [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();

	        _scheduleRepository = new FakeScheduleRepository();
		    var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
		    _target = new EnsureWeeklyRestRule(workTimeStartEndExtractor, new DayOffMaxFlexCalculator(workTimeStartEndExtractor));
        }

	    [Test]
		public void ReturnTrueIfNoPersonAssignmentFound()
	    {
		    var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
		    var personWeek = new PersonWeek(_person, week);

		    var result = _target.HasMinWeeklyRest(personWeek,
			    _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
				    new ScheduleDictionaryLoadOptions(false, false), personWeek.Week.Inflate(2),
				    ScenarioFactory.CreateScenarioAggregate())[_person], TimeSpan.FromHours(10));
		    Assert.IsTrue(result);
	    }

	    [Test]
	    public void ReturnFalseIfTheWeekHasInValidWeeklyRest()
	    {
		    var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
		    var personWeek = new PersonWeek(_person, week);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
		    var activity = ActivityFactory.CreateActivity("Phone");
		    activity.InWorkTime = true;

		    _scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _person,new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory(), scenario));
		    _scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _person,new DateTimePeriod(2014, 03, 20, 5, 2014, 03, 20, 16), ShiftCategoryFactory.CreateShiftCategory(), scenario));

		    var result = _target.HasMinWeeklyRest(personWeek,
			    _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
				    new ScheduleDictionaryLoadOptions(false, false), personWeek.Week.Inflate(2),
				    scenario)[_person], TimeSpan.FromHours(40));
		    Assert.IsFalse(result);
	    }

		[Test]
		public void ReturnTrueIfTheWeekHasValidWeeklyRest()
		{
			var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
			var personWeek = new PersonWeek(_person, week);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _person, new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory(), scenario));
			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _person, new DateTimePeriod(2014, 03, 20, 5, 2014, 03, 20, 16), ShiftCategoryFactory.CreateShiftCategory(), scenario));

			var result = _target.HasMinWeeklyRest(personWeek,
				_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
					new ScheduleDictionaryLoadOptions(false, false), personWeek.Week.Inflate(2),
					scenario)[_person], TimeSpan.FromHours(10));
			Assert.IsTrue(result);
		}

		[Test]
		public void ReturnTrueIfTheWeekHasUnscheduleDay()
		{
			var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
			var personWeek = new PersonWeek(_person, week);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _person, new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory(), scenario));
			
			var result = _target.HasMinWeeklyRest(personWeek,
				_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
					new ScheduleDictionaryLoadOptions(false, false), personWeek.Week.Inflate(2),
					scenario)[_person], TimeSpan.FromHours(40));
			Assert.IsTrue(result);
		}
    }

	public static class DateOnlyPeriodForTestExtensions
	{
		public static DateOnlyPeriod Inflate(this DateOnlyPeriod period, int days)
		{
			return new DateOnlyPeriod(period.StartDate.AddDays(-days),period.EndDate.AddDays(days));
		}
	}
}
