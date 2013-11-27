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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SwapServiceTest
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
            IList<IScheduleDay> list = new List<IScheduleDay>();
            list.Add(_p1D1);
            SwapService service = new SwapService();
            service.Init(list);
            Assert.IsFalse(service.CanSwapAssignments());
            list.Add(_p2D1);
            list.Add(_p2D2);
            service = new SwapService();
            service.Init(list);
            Assert.IsFalse(service.CanSwapAssignments());
            //Assert.IsFalse(service.CanSwapDaysOff());
        }

        [Test]
        public void VerifyCanSwapAssignmentsWhenDateIsNotTheSame()
        {
            IList<IScheduleDay> list = new List<IScheduleDay>();
            _p1D2.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d2));
            _p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
            list.Add(_p2D1);
            list.Add(_p1D2);
            SwapService service = new SwapService();
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
            IList<IScheduleDay> list = new List<IScheduleDay>();
            list.Add(_p1D1);
            list.Add(_p2D2);
            SwapService service = new SwapService();
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

            SwapService service = new SwapService();
            service.Init(_list);
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            IList<IScheduleDay> retList = service.SwapAssignments(_dictionary);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySwapAssignmentsWithEmptyDaysInvolvedWorks()
        {
            _list = new List<IScheduleDay>();

            _p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
            _list.Add(_p1D1);
            _list.Add(_p2D1);

            DateTimePeriod period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
            _dictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period));
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            assignments.Add(_p1D1.PersonAssignment());
            ((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
            assignments = new List<IPersonAssignment>();
            ((ScheduleRange)_dictionary[_person2]).AddRange(assignments);
            SwapService service = new SwapService();
            service.Init(_list);
            Assert.AreEqual("kalle", _list[0].Person.Name.LastName);

            using (_mocks.Record())
            {
                _mocks.BackToRecord(_dic);
                Expect.Call(_dic[null]).IgnoreArguments().Return(_range).Repeat.AtLeastOnce();
            }
            IList<IScheduleDay> retList = service.SwapAssignments(_dictionary);

            Assert.AreEqual("kalle", retList[0].Person.Name.LastName);
						Assert.AreEqual(0, retList[0].PersonAssignment().MainLayers().Count());
        }

		[Test]
		public void Swap_WhenTheAssignmentsAreInDifferentTimeZones_ShouldNotAffectTheAssignments()
		{

			var scenario = new Scenario("scenario");
			var skill = SkillFactory.CreateSkill("skill");
			var firstTimeZone = TimeZoneInfo.CreateCustomTimeZone("first", TimeSpan.Zero, "first", "second");
			var secondTimeZone = TimeZoneInfo.CreateCustomTimeZone("second", TimeSpan.FromHours(5), "second", "second");

			var firstPerson = PersonFactory.CreatePerson("FirstPerson");
			var secondPerson = PersonFactory.CreatePerson("SecondPerson");

			var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
			var scheduleFactory1 = new SchedulePartFactoryForDomain(firstPerson,scenario,period,skill,firstTimeZone);
			var scheduleFactory2 = new SchedulePartFactoryForDomain(secondPerson,scenario,period,skill,secondTimeZone);
			var scheduleDay1 = scheduleFactory1.CreatePartWithMainShift();
			var scheduleDay2 = scheduleFactory2.CreatePartWithMainShift();
			var target = new SwapService();

			var schedules = new List<IScheduleDay>() {scheduleDay1, scheduleDay2};
			target.Init(schedules);

			verifyAllSchedulesStartAtTheSameUtcTime(schedules);

			var dic = new ScheduleDictionaryForTest(scenario, period);
			
			dic.AddPersonAssignment(scheduleDay1.PersonAssignment());
			dic.AddPersonAssignment(scheduleDay2.PersonAssignment());
			IList<IScheduleDay> swapped = target.SwapAssignments(dic);

			verifyAllSchedulesStartAtTheSameUtcTime(swapped);

		}

		private static void verifyAllSchedulesStartAtTheSameUtcTime(IEnumerable<IScheduleDay> schedules)
		{
			var mainShifts = from s in schedules
			                 select s.PersonAssignment().MainLayers().First().Period.StartDateTime;
			
			Assert.That(mainShifts.Distinct().Count(),Is.EqualTo(1),"All mainshiftlayers expected to start at the same time");
		}

        [Test, ExpectedException(typeof(ConstraintException))]
        public void VerifyInvalidList()
        {
            _list = new List<IScheduleDay>();
            SwapService service = new SwapService();
            service.Init(_list);
            service.SwapAssignments(_dictionary);
        }
        
        private void SetupForAssignmentSwap()
        {
            _list = new List<IScheduleDay>();

            _p1D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1, _d1));
            _p2D1.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2, _d1));
            _list.Add(_p1D1);
            _list.Add(_p2D1);

            DateTimePeriod period = new DateTimePeriod(_d1.StartDateTime, _d2.EndDateTime);
            _dictionary = 
                new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period),
                                       new DifferenceEntityCollectionService<IPersistableScheduleData>());
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            assignments.Add(_p1D1.PersonAssignment());
            ((ScheduleRange)_dictionary[_person1]).AddRange(assignments);
            assignments = new List<IPersonAssignment>();
            assignments.Add(_p2D1.PersonAssignment());
            ((ScheduleRange)_dictionary[_person2]).AddRange(assignments);

        }

    }


	
}