﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
	[DomainTest]
	public class EnsureWeeklyRestRuleTest
    {
        private IEnsureWeeklyRestRule _target;
        private IPerson _person;
	    private FakeScheduleStorage_DoNotUse _scheduleStorage;

	    [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();

	        _scheduleStorage = new FakeScheduleStorage_DoNotUse();
		    var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
		    _target = new EnsureWeeklyRestRule(workTimeStartEndExtractor, new DayOffMaxFlexCalculator(workTimeStartEndExtractor));
        }

	    [Test]
		public void ReturnTrueIfNoPersonAssignmentFound()
	    {
		    var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
		    var personWeek = new PersonWeek(_person, week);

		    var result = _target.HasMinWeeklyRest(personWeek,
			    _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person,
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

		    _scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, scenario, activity, new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory()));
		    _scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, scenario, activity, new DateTimePeriod(2014, 03, 20, 5, 2014, 03, 20, 16), ShiftCategoryFactory.CreateShiftCategory()));

		    var result = _target.HasMinWeeklyRest(personWeek,
			    _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person,
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

			_scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, scenario, activity, new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory()));
			_scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, scenario, activity, new DateTimePeriod(2014, 03, 20, 5, 2014, 03, 20, 16), ShiftCategoryFactory.CreateShiftCategory()));

			var result = _target.HasMinWeeklyRest(personWeek,
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person,
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

			_scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, scenario, activity, new DateTimePeriod(2014, 03, 19, 5, 2014, 03, 19, 16), ShiftCategoryFactory.CreateShiftCategory()));
			
			var result = _target.HasMinWeeklyRest(personWeek,
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person,
					new ScheduleDictionaryLoadOptions(false, false), personWeek.Week.Inflate(2),
					scenario)[_person], TimeSpan.FromHours(40));
			Assert.IsTrue(result);
		}
    }
}
