using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
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

        [SetUp]
        public void Setup()
        {
             var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
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
        }

        [Test]
        public void VerifyCanSwapOnlyWhenTwoSelected()
        {
            IList<IScheduleDay> list = new List<IScheduleDay> {_p1D1};
        	var service = new SwapServiceNew();
            service.Init(list);
            Assert.IsFalse(service.CanSwapAssignments());
            list.Add(_p2D1);
            list.Add(_p2D2);
            service = new SwapServiceNew();
            service.Init(list);
            Assert.IsFalse(service.CanSwapAssignments());
        }

        [Test]
        public void VerifyCanSwapAssignmentsWhenDateIsNotTheSame()
        {
            IList<IScheduleDay> list = new List<IScheduleDay>();
            _p1D2.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d2));
            _p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
            list.Add(_p2D1);
            list.Add(_p1D2);
            var service = new SwapServiceNew();
            service.Init(list);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            Assert.IsTrue(service.CanSwapAssignments());

        }

        [Test]
        public void VerifyCanSwapAssignmentsWhenDifferentAgentsOnDifferentDays()
        {
            IList<IScheduleDay> list = new List<IScheduleDay> {_p1D1, _p2D2};
        	var service = new SwapServiceNew();
            service.Init(list);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            Assert.IsTrue(service.CanSwapAssignments());
        }

        [Test]
        public void VerifySwapAssignmentsWorks()
        {
            SetupForAssignmentSwap();

            var service = new SwapServiceNew();
            service.Init(_list);
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            var retList = service.Swap(_dictionary);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySwapAssignmentsWithEmptyDaysInvolvedWorks()
        {
            _list = new List<IScheduleDay>();

            _p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
            _list.Add(_p1D1);
            _list.Add(_p2D1);

            var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
            _dictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period));
            IList<IPersonAssignment> assignments = new List<IPersonAssignment> {_p1D1.PersonAssignmentCollection()[0]};
        	((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
            assignments = new List<IPersonAssignment>();
            ((ScheduleRange)_dictionary[_person2]).AddRange(assignments);
            var service = new SwapServiceNew();
            service.Init(_list);
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            var retList = service.Swap(_dictionary);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
            Assert.AreEqual(0, retList[0].PersonAssignmentCollection().Count);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSwapEmptyDayWithDayOff()
		{
			_list = new List<IScheduleDay>();

			_p1D1.Add(PersonDayOffFactory.CreatePersonDayOff(_person1, _scenario, new DateOnly(_d1.StartDateTime), TimeSpan.FromHours(24), TimeSpan.FromHours(0), TimeSpan.FromHours(12)));
			
			_list.Add(_p1D1);
			_list.Add(_p2D1); // empty day

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period));
			IList<IPersonDayOff> personDayOffs = new List<IPersonDayOff> {_p1D1.PersonDayOffCollection()[0]};
			((ScheduleRange)_dictionary[_person1]).AddRange(personDayOffs);
			
			IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
			((ScheduleRange)_dictionary[_person2]).AddRange(personAssignments);

			var service = new SwapServiceNew();
			service.Init(_list);
			Assert.AreEqual("kalle", _list[0].Person.Name.LastName);
			Assert.AreEqual(1, _list[0].PersonDayOffCollection().Count());

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}

			var retList = service.Swap(_dictionary);

			Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
			Assert.AreEqual(0, retList[0].PersonDayOffCollection().Count);
			Assert.AreEqual(1, retList[1].PersonDayOffCollection().Count);		
		}

        [Test, ExpectedException(typeof(ConstraintException))]
        public void VerifyInvalidList()
        {
            _list = new List<IScheduleDay>();
            var service = new SwapServiceNew();
            service.Init(_list);
            service.Swap(_dictionary);
        }

        private void SetupForAssignmentSwap()
        {
            _list = new List<IScheduleDay>();

            _p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
            _p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
            _list.Add(_p1D1);
            _list.Add(_p2D1);

            var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
            _dictionary = 
                new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
                                       new DifferenceEntityCollectionService<IPersistableScheduleData>());
            IList<IPersonAssignment> assignments = new List<IPersonAssignment> {_p1D1.PersonAssignmentCollection()[0]};
        	((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
            assignments = new List<IPersonAssignment> {_p2D1.PersonAssignmentCollection()[0]};
        	((ScheduleRange)_dictionary[_person2]).AddRange(assignments);

        }

		/// <summary>
		/// WHEN swapping an absence day with a shift day, THEN full day absence should not disappear
		/// </summary>
		[Test]
		public void ShouldNotDisappearFullDayAbsence()
		{
			IList<IScheduleDay> _list = new List<IScheduleDay>();
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>());
			IList<IPersonAssignment> assignments = new List<IPersonAssignment> { _p1D1.PersonAssignmentCollection()[0] };
			((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
			assignments = new List<IPersonAssignment> { _p2D1.PersonAssignmentCollection()[0] };
			((ScheduleRange)_dictionary[_person2]).AddRange(assignments);

			var service = new SwapServiceNew();
			service.Init(_list);

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments());
			var retList = service.Swap(_dictionary);

			Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
			
		}

		/// <summary>
		/// WHEN swapping an absence day with a shift day, THEN short absence should not disappear
		/// </summary>
		[Test]
		public void ShouldNotDisappearShortAbsence()
		{
		}

		/// <summary>
		/// WHEN swapping an absence day with a shift day, THEN overtime should not disappear
		/// </summary>
		[Test]
		public void ShouldNotDisappearOvertime()
		{
		}

		/// <summary>
		/// WHEN swapping an day with another shift day, THEN the overtime should move
		/// </summary>
		[Test]
		public void ShouldMoveOvertime()
		{
			IList<IScheduleDay> _list = new List<IScheduleDay>();
			_p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(_scenario, _person1, _d1));
			_p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
			_list.Add(_p1D1);
			_list.Add(_p2D1);

			var period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
			_dictionary =
				new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
									   new DifferenceEntityCollectionService<IPersistableScheduleData>());
			IList<IPersonAssignment> p1assignments = new List<IPersonAssignment> { _p1D1.PersonAssignmentCollection()[0] };
			((ScheduleRange)_dictionary[_person1]).AddRange(p1assignments);
			IList<IPersonAssignment> p2assignments = new List<IPersonAssignment> { _p2D1.PersonAssignmentCollection()[0] };
			((ScheduleRange)_dictionary[_person2]).AddRange(p2assignments);

			Assert.AreEqual(1, p1assignments[0].OvertimeShiftCollection.Count());
			Assert.AreEqual(0, p2assignments[0].OvertimeShiftCollection.Count());

			var service = new SwapServiceNew();
			service.Init(_list);

			using (_mocks.Record())
			{
				_mocks.BackToRecord(_dic);
				Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
			}
			Assert.IsTrue(service.CanSwapAssignments());
			var retList = service.Swap(_dictionary);

			Assert.AreEqual(_person1.Name.LastName, retList[0].Person.Name.LastName);
			Assert.AreEqual(_person2.Name.LastName, retList[1].Person.Name.LastName);

			var p1assignmentsAfterSwap = retList[0].PersonAssignmentCollection()[0];
			var p2assignmentsAfterSwap = retList[1].PersonAssignmentCollection()[0];

			Assert.AreEqual(0, p1assignmentsAfterSwap.OvertimeShiftCollection.Count());
			Assert.AreEqual(1, p2assignmentsAfterSwap.OvertimeShiftCollection.Count());
		}

		/// <summary>
		/// WHEN swapping an day with another shift day, THEN the short absence should move
		/// </summary>
		[Test]
		public void ShouldMoveShortAbsence()
		{
		}

		/// <summary>
		/// WHEN swapping more a set of absence days with a set of shift days, THEN full day absence should not disappear from the last day
		/// </summary>
		[Test]
		public void ShouldNotDisappearFullDayAbsenceFromTheLastDay()
		{
		}

		/// <summary>
		/// WHEN swapping more a set of absence days with a set of shift days, THEN full day absences should not get doubled on the first day
		/// </summary>
		[Test]
		public void ShouldNotHaveDoubleFullDayAbsenceOnTheFirstDay()
		{
		}

    }
}
