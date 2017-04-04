using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class SwapServiceNewTest
	{
		private IPerson _person1;
        private IPerson _person2;
        private IScenario _scenario;
        private DateTimePeriod _d1;
        private DateTimePeriod _d2;
        private IScheduleDay _p1D1;
        private IScheduleDay _p1D2;
        private IScheduleDay _p2D1;
        private IScheduleDay _p2D2;
        private IList<IScheduleDay> _list;
        private IScheduleDictionary _dictionary;
        private MockRepository _mocks;
        private IScheduleRange _range;
        private IScheduleDictionary _dic;
		private IPersistableScheduleDataPermissionChecker _permissionChecker;

        [SetUp]
        public void Setup()
        {
             var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            _mocks = new MockRepository();
            _scenario = new Scenario("hej");
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _range = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(_dic.Scenario).Return(_scenario).Repeat.Any();
            Expect.Call(_dic.PermissionsEnabled).Return(true).Repeat.Any();
            _mocks.Replay(_dic);
            _person1 = PersonFactory.CreatePerson("kalle");
            _person1.SetId(Guid.NewGuid());
            _person1.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            _person2 = PersonFactory.CreatePerson("pelle");
            _person2.SetId(Guid.NewGuid());
            _person2.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            _d1 = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            _d2 = new DateTimePeriod(new DateTime(2008, 2, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 2, 3, 0, 0, 0, DateTimeKind.Utc));
            _p1D1 = ExtractedSchedule.CreateScheduleDay(_dic, _person1, new DateOnly(2008,1,1));
            _p1D2 = ExtractedSchedule.CreateScheduleDay(_dic, _person1, new DateOnly(2008, 2, 2));
            _p2D1 = ExtractedSchedule.CreateScheduleDay(_dic, _person2, new DateOnly(2008, 1, 1));
            _p2D2 = ExtractedSchedule.CreateScheduleDay(_dic, _person2, new DateOnly(2008, 2, 2));
			_permissionChecker = new PersistableScheduleDataPermissionChecker();
        }

        [Test]
        public void VerifyCanSwapOnlyWhenTwoSelected()
        {
            IList<IScheduleDay> list = new List<IScheduleDay> {_p1D1};
        	var service = new SwapServiceNew();
            Assert.IsFalse(service.CanSwapAssignments(list));
            list.Add(_p2D1);
            list.Add(_p2D2);
            service = new SwapServiceNew();
            Assert.IsFalse(service.CanSwapAssignments(list));
        }

        [Test]
        public void VerifyCanSwapAssignmentsWhenDateIsNotTheSame()
        {
            IList<IScheduleDay> list = new List<IScheduleDay>();
            _p1D2.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person1, _scenario, _d2));
            _p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
            list.Add(_p2D1);
            list.Add(_p1D2);
            var service = new SwapServiceNew();

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            Assert.IsTrue(service.CanSwapAssignments(list));

        }

        [Test]
        public void VerifyCanSwapAssignmentsWhenDifferentAgentsOnDifferentDays()
        {
            IList<IScheduleDay> list = new List<IScheduleDay> {_p1D1, _p2D2};
        	var service = new SwapServiceNew();

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            Assert.IsTrue(service.CanSwapAssignments(list));
        }

		[Test]
		public void VerifyInvalidList()
		{
			_list = new List<IScheduleDay>();
			var service = new SwapServiceNew();
			Assert.Throws<ConstraintException>(() => service.Swap(_dictionary, _list));
		}

        [Test]
        public void ShouldSwapMainShifts()
        {
			_list = new List<IScheduleDay>();

			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person1, _scenario, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAssignment> assignments = new List<IPersonAssignment> { _p1D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
			assignments = new List<IPersonAssignment> { _p2D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person2]).AddRange(assignments);

            var service = new SwapServiceNew();
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            var retList = service.Swap(_dictionary, _list);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
        }

        [Test]
        public void ShouldSwapMainShiftWithEmptyDay()
        {
            _list = new List<IScheduleDay>();

            _p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person1, _scenario, _d1));
            _list.Add(_p1D1);
            _list.Add(_p2D1);

            var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
            _dictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period), _permissionChecker);
            IList<IPersonAssignment> assignments = new List<IPersonAssignment> {_p1D1.PersonAssignment()};
        	((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
            assignments = new List<IPersonAssignment>();
            ((ScheduleRange)_dictionary[_person2]).AddRange(assignments);
            var service = new SwapServiceNew();
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            var retList = service.Swap(_dictionary, _list);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
						Assert.AreEqual(0, retList[0].PersonAssignment().MainActivities().Count());
        }

		[Test]
		public void ShouldSwapDayOffWithEmptyDay()
		{
			_list = new List<IScheduleDay>();

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person1, _scenario, new DateOnly(_d1.StartDateTime), TimeSpan.FromHours(24), TimeSpan.FromHours(0), TimeSpan.FromHours(12));
			_p1D1.Add(dayOff);
			
			_list.Add(_p1D1);
			_list.Add(_p2D1); // empty day

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period), _permissionChecker);
			((ScheduleRange)_dictionary[_person1]).Add(dayOff);
			
			IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
			((ScheduleRange)_dictionary[_person2]).AddRange(personAssignments);

			var service = new SwapServiceNew();
			Assert.AreEqual("kalle", _list[0].Person.Name.LastName);
			Assert.IsNotNull(_list[0].PersonAssignment().DayOff());
		
			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}

			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
			retList[0].HasDayOff().Should().Be.False();
			retList[1].HasDayOff().Should().Be.True();		
		}

		/// <summary>
		/// WHEN swapping an absence day with a mainshift, THEN full day absence should stay
		/// </summary>
		[Test]
		public void ShouldStayFullDayAbsenceAndMoveMainshiftWhenSwapAbsenceWithMainShift()
		{

			IList<IScheduleDay> _list = new List<IScheduleDay>();

			var absencePeriod = new DateTimePeriod(_d1.StartDateTime, _d1.EndDateTime);
			_p1D1.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person1, _scenario, absencePeriod));
			_p1D1.Add(PersonAbsenceFactory.CreatePersonAbsence(_person1, _scenario, absencePeriod));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
			// NOTE that we need to create an empty assignment from 388 as there is no null PersonAssignment() any longer
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);

			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAssignment> p1assignments = new List<IPersonAssignment> { _p1D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1assignments);
			IList<IPersonAbsence> p1absences = new List<IPersonAbsence> { _p1D1.PersonAbsenceCollection()[0] };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1absences);
			IList<IPersonAssignment> p2assignments = new List<IPersonAssignment> { _p2D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person2]).AddRange(p2assignments);

			Assert.AreEqual(0, _p1D1.PersonAssignment().MainActivities().Count());
			Assert.AreEqual(1, _p1D1.PersonAbsenceCollection().Count);
			// NOTE that the followith case is not right from the edition 388 as there is no null PersonAssignment() any longer
			//Assert.IsNull(_p2D1.PersonAssignment());
			Assert.AreEqual(0, _p2D1.PersonAbsenceCollection().Count);

			var service = new SwapServiceNew();

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic.PermissionsEnabled).Return(true).Repeat.Any();
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments(_list));
			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			Assert.AreEqual(1, retList[0].PersonAbsenceCollection().Count());
			Assert.AreEqual(0, retList[1].PersonAbsenceCollection().Count());

			Assert.AreEqual(1, retList[0].PersonAssignment().MainActivities().Count());
			Assert.AreEqual(0, retList[1].PersonAssignment().MainActivities().Count());
		}

		/// <summary>
		/// WHEN swapping an absence day with an empty day, THEN absence should stay
		/// </summary>
		/// <remarks>Bug 24260</remarks>
		[Test]
		public void ShouldStayAbsenceWhenSwapWithEmptyDay()
		{

			IList<IScheduleDay> _list = new List<IScheduleDay>();
			_d1 = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-9), new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc).AddHours(-9));

			var absencePeriod = new DateTimePeriod(_d1.StartDateTime, _d1.EndDateTime.AddMinutes(-1));

			_p1D1.Add(PersonAbsenceFactory.CreatePersonAbsence(_person1, _scenario, absencePeriod));
			_list.Add(_p1D1);
			_list.Add(_p2D1); // empty day

			var period = new DateTimePeriod(_d1.StartDateTime, _d1.EndDateTime);

			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAbsence> p1absences = new List<IPersonAbsence> { _p1D1.PersonAbsenceCollection()[0] };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1absences);

			IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
			((ScheduleRange)_dictionary[_person2]).AddRange(personAssignments);

			Assert.IsNull(_p1D1.PersonAssignment());
			Assert.AreEqual(1, _p1D1.PersonAbsenceCollection().Count);
			Assert.IsNull(_p2D1.PersonAssignment());
			Assert.AreEqual(0, _p2D1.PersonAbsenceCollection().Count);

			var service = new SwapServiceNew();

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic.PermissionsEnabled).Return(true).Repeat.Any();
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments(_list));
			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			Assert.AreEqual(1, retList[0].PersonAbsenceCollection().Count());
			Assert.AreEqual(0, retList[1].PersonAbsenceCollection().Count());
		}


		/// <summary>
		/// WHEN swapping an absence day with a main day, THEN short absence should not disappear, but stay
		/// </summary>
		[Test]
		public void ShouldStayShortAbsenceWhenSwapWithMainShift()
		{
			IList<IScheduleDay> _list = new List<IScheduleDay>();

			var absencePeriod = new DateTimePeriod(_d1.StartDateTime.AddHours(3), _d1.StartDateTime.AddHours(4));
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person1, _scenario, _d1));
			_p1D1.Add(PersonAbsenceFactory.CreatePersonAbsence(_person1, _scenario, absencePeriod));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);

			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAssignment> p1assignments = new List<IPersonAssignment> { _p1D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1assignments);
			IList<IPersonAbsence> p1absences = new List<IPersonAbsence> { _p1D1.PersonAbsenceCollection()[0] };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1absences);
			IList<IPersonAssignment> p2assignments = new List<IPersonAssignment> { _p2D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person2]).AddRange(p2assignments);

			Assert.IsNotNull(_p1D1.PersonAssignment());
			Assert.AreEqual(1, _p1D1.PersonAbsenceCollection().Count);
			Assert.IsNotNull(_p2D1.PersonAssignment());
			Assert.AreEqual(0, _p2D1.PersonAbsenceCollection().Count);

			var service = new SwapServiceNew();

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic.PermissionsEnabled).Return(true).Repeat.Any();
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments(_list));
			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			Assert.AreEqual(1, retList[0].PersonAbsenceCollection().Count());
			Assert.AreEqual(0, retList[1].PersonAbsenceCollection().Count());	
		}

		/// <summary>
		/// WHEN swapping an day with another shift day, THEN the personal shift should stay
		/// </summary>
		[Test]
		public void ShouldNotSwapPersonalShift()
		{
			IList<IScheduleDay> _list = new List<IScheduleDay>();
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person1, _scenario, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAssignment> p1assignments = new List<IPersonAssignment> { _p1D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1assignments);
			IList<IPersonAssignment> p2assignments = new List<IPersonAssignment> { _p2D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person2]).AddRange(p2assignments);

			Assert.AreEqual(1, p1assignments[0].PersonalActivities().Count());
			Assert.AreEqual(0, p2assignments[0].PersonalActivities().Count());

			var service = new SwapServiceNew();

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments(_list));
			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			var p1assignmentsAfterSwap = retList[0].PersonAssignment();
			var p2assignmentsAfterSwap = retList[1].PersonAssignment();

			Assert.AreEqual(1, p1assignmentsAfterSwap.PersonalActivities().Count());
			Assert.AreEqual(0, p2assignmentsAfterSwap.PersonalActivities().Count());
		}

		/// <summary>
		/// WHEN swapping an day with another shift day, THEN the overtime should move
		/// </summary>
		[Test]
		public void ShouldSwapOvertime()
		{
			IList<IScheduleDay> _list = new List<IScheduleDay>();
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(_person1, _scenario, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2, _scenario, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>(), _permissionChecker);
			IList<IPersonAssignment> p1assignments = new List<IPersonAssignment> { _p1D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1assignments);
			IList<IPersonAssignment> p2assignments = new List<IPersonAssignment> { _p2D1.PersonAssignment() };
			((ScheduleRange)_dictionary[_person2]).AddRange(p2assignments);

			Assert.AreEqual(1, p1assignments[0].OvertimeActivities().Count());
			Assert.AreEqual(0, p2assignments[0].OvertimeActivities().Count());

			var p1Period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_d1.StartDateTime.AddDays(-1)));
			_person1.AddPersonPeriod(p1Period);
			var p2Period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_d1.StartDateTime.AddDays(-1)));
			_person2.AddPersonPeriod(p2Period);

			var definitionSet = p1assignments[0].OvertimeActivities().ToList()[0].DefinitionSet;
			_person1.PersonPeriodCollection[0].PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			_person2.PersonPeriodCollection[0].PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet);

			var service = new SwapServiceNew();

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments(_list));
			var retList = service.Swap(_dictionary, _list);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			var p1assignmentsAfterSwap = retList[0].PersonAssignment();
			var p2assignmentsAfterSwap = retList[1].PersonAssignment();

			Assert.AreEqual(0, p1assignmentsAfterSwap.OvertimeActivities().Count());
			Assert.AreEqual(1, p2assignmentsAfterSwap.OvertimeActivities().Count());
		}
    }
}
