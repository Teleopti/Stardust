using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SwapServiceNewTest
	{
		private IPerson _person1;
        private IPerson _person2;
        private IScenario _scenario;
        private DateTimePeriod _d1;
        private IScheduleDay _p1D1;
        private IScheduleDay _p2D1;
        private IScheduleDictionary _dictionary;
		private ISwapServiceNew _target;

        [SetUp]
        public void Setup()
        {
             var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            _scenario = new Scenario("hej");
			_target = new SwapServiceNew();

            _person1 = PersonFactory.CreatePerson("kalle");
            _person1.SetId(Guid.NewGuid());
            _person1.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            _person2 = PersonFactory.CreatePerson("pelle");
            _person2.SetId(Guid.NewGuid());
            _person2.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            _d1 = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(_d1),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>());
			_p1D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2008, 1, 1));
			_p2D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2008, 1, 1));

        }

		[Test]
		public void MainShiftShouldBeSwappedPersonalShiftStays()
		{
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithPersonalShift(ActivityFactory.CreateActivity("hej"), _person2,
			                                                                    _d1, _scenario));

			assignScheduleDays();

			var result = _target.Swap(_p1D1, _p2D1, _dictionary);
			Assert.That(result[1].PersonAssignmentCollection()[0].MainShift != null);
			Assert.That(result[1].PersonAssignmentCollection()[0].PersonalShiftCollection.Count == 1);
			Assert.That(result[0].PersonAssignmentCollection().Count == 0);

		}

		[Test]
		public void ShouldSwapEmptyDayWithDayOff()
		{
			_p1D1.Add(PersonDayOffFactory.CreatePersonDayOff(_person1, _scenario, _d1.StartDateTime,
			                                                 TimeSpan.FromHours(24), TimeSpan.FromHours(0),
			                                                 TimeSpan.FromHours(12)));

			assignScheduleDays();

			var result = _target.Swap(_p1D1, _p2D1, _dictionary);
			Assert.That(result[1].PersonDayOffCollection().Count == 1);
			Assert.That(result[0].PersonDayOffCollection().Count == 0);

		}

		[Test]
		public void SwappingDayOffToADayWithAbsenceShouldLeaveTheAbsence()
		{
			_p1D1.Add(PersonDayOffFactory.CreatePersonDayOff(_person1, _scenario, _d1.StartDateTime,
															 TimeSpan.FromHours(24), TimeSpan.FromHours(0),
															 TimeSpan.FromHours(12)));
			_p2D1.Add(PersonAbsenceFactory.CreatePersonAbsence(_person2, _scenario, _d1));

			assignScheduleDays();

			var result = _target.Swap(_p1D1, _p2D1, _dictionary);
			Assert.That(result[1].PersonDayOffCollection().Count == 1);
			Assert.That(result[1].PersonAbsenceCollection().Count == 1);
			Assert.That(result[0].PersonDayOffCollection().Count == 0);
			Assert.That(result[0].PersonAbsenceCollection().Count == 0);
		}

		[Test]
		public void OvertimeShouldSwapIfSameMultiplicatorDefinition()
		{
			var activity1 = ActivityFactory.CreateActivity("hej");
			var activity2 = ActivityFactory.CreateActivity("hopp");
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();
			personContract.Contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
			personPeriod.PersonContract = personContract;

			_person1.AddPersonPeriod(personPeriod);
			_person2.AddPersonPeriod(personPeriod);

			IPersonAssignment ass1 = new PersonAssignment(_person1, _scenario);
			ass1.AddOvertimeShift(OvertimeShiftFactory.CreateOvertimeShift(activity1, _d1, multiplicatorDefinitionSet, ass1));
			_p1D1.Add(ass1);

			IPersonAssignment ass2 = new PersonAssignment(_person2, _scenario);
			ass2.AddOvertimeShift(OvertimeShiftFactory.CreateOvertimeShift(activity2, _d1, multiplicatorDefinitionSet, ass2));
			_p2D1.Add(ass2);

			assignScheduleDays();

			var result = _target.Swap(_p1D1, _p2D1, _dictionary);
			Assert.That(result[1].PersonAssignmentCollection()[0].OvertimeShiftCollection[0].LayerCollection[0].Payload.Equals(activity1));
			Assert.That(result[0].PersonAssignmentCollection()[0].OvertimeShiftCollection[0].LayerCollection[0].Payload.Equals(activity2));
		}

		private void assignScheduleDays()
		{
			((ScheduleRange)_dictionary[_person1]).ModifyInternal(_p1D1);
			((ScheduleRange)_dictionary[_person2]).ModifyInternal(_p2D1);
		}
    }
}
