using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class ScheduleHelperTest
    {
        private IList<IScheduleDay> _schedules;
        private IScenario _scenario;

        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;

        private IScheduleDay _schedulePart1;
        private IScheduleDay _schedulePart11;
        private IScheduleDay _schedulePart111;

        private IScheduleDay _schedulePart2;
        private IScheduleDay _schedulePart22;
        private IScheduleDay _schedulePart222;

        private IScheduleDay _schedulePart3;
        private IScheduleDay _schedulePart33;
        private IScheduleDay _schedulePart333;

        private IScheduleDay _schedulePart4;
        private IScheduleDay _schedulePart44;

		private IScheduleDay _schedulePart5;

        private IPersonAssignment _ass1;
        private IPersonAssignment _ass2;
        private IPersonAssignment _ass3;
		private IPersonAssignment _ass4;

        private IPersonAssignment _dayOff1;
				private IPersonAssignment _dayOff2;
				private IPersonAssignment _dayOff3;

        private PersonAbsence _abs1;
        private PersonAbsence _abs2;
        private PersonAbsence _abs3;

        private DateTimePeriod _period1;
        private DateTimePeriod _period2;
        private DateTimePeriod _period3;

        private DateTime _date1;
        private DateTime _date2;
        private DateTime _date3;

        private ForTestShiftCategory _shiftCategory1;
        private ForTestShiftCategory _shiftCategory2;

        ForTestAbsence _ab1;
        ForTestAbsence _ab2;
        ForTestAbsence _ab3;

        private ForTestDayOffTemplate _dayOffTemplate;
        MockRepository _mocks;
        private IScheduleDictionary dic;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedules = new List<IScheduleDay>();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _person1 = PersonFactory.CreatePerson();
            _person2 = PersonFactory.CreatePerson();
            _person3 = PersonFactory.CreatePerson();

            dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_period1),
                                                new Dictionary<IPerson, IScheduleRange>());
            
            _shiftCategory1 = new ForTestShiftCategory("ShiftCategory1", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            _shiftCategory2 = new ForTestShiftCategory("ShiftCategory2", "baaaaaaaaaaaaaaaaaaaaaaaaaaaaaab");

            _ab1 = new ForTestAbsence("111aaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            _ab2 = new ForTestAbsence("222aaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            _ab3 = new ForTestAbsence("333aaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            _dayOffTemplate = new ForTestDayOffTemplate(Guid.NewGuid().ToString());

            _person1.SetName(new Name("Person", "1"));
            _person2.SetName(new Name("Person", "2"));
            _person3.SetName(new Name("Person", "3"));

            _period1 = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            _period2 = new DateTimePeriod(2000, 1, 2, 2000, 1, 3);
            _period3 = new DateTimePeriod(2000, 1, 3, 2000, 1, 4);

            _date1 = new DateTime(2000, 1, 1);
            _date2 = new DateTime(2000, 1, 2);
            _date3 = new DateTime(2000, 1, 4);

            _ass1 = CreatePersonAssignment(_period1, _person1, _shiftCategory1);
            _ass2 = CreatePersonAssignment(_period2, _person2, _shiftCategory2);
            _ass3 = CreatePersonAssignment(_period3, _person3, _shiftCategory2);
			_ass4 = createPersonAssignmentWithNoMainShiftButOvertime(_period3, _person3);

            _dayOff1 = CreatePersonDayOff(_date2, _person1);
            _dayOff2 = CreatePersonDayOff(_date1, _person2);
            _dayOff3 = CreatePersonDayOffWithSpecificTemplate(_date3, _person3, _dayOffTemplate);

            _abs1 = CreatePersonAbsence(_period3, _person1, _ab1);
            _abs2 = CreatePersonAbsence(_period3, _person2, _ab2);
            _abs3 = CreatePersonAbsence(_period3, _person3, _ab3);
			
			var currentAuthorization = new FullPermission();
			_schedulePart1 = ExtractedSchedule.CreateScheduleDay(dic, _person1, new DateOnly(2000,1,1), currentAuthorization);
            _schedulePart11 = ExtractedSchedule.CreateScheduleDay(dic, _person1, new DateOnly(2000, 1, 2), currentAuthorization);
            _schedulePart111 = ExtractedSchedule.CreateScheduleDay(dic, _person1, new DateOnly(2000, 1, 3), currentAuthorization);

            _schedulePart2 = ExtractedSchedule.CreateScheduleDay(dic, _person2, new DateOnly(2000, 1, 1), currentAuthorization);
            _schedulePart22 = ExtractedSchedule.CreateScheduleDay(dic, _person2, new DateOnly(2000, 1, 2), currentAuthorization);
            _schedulePart222 = ExtractedSchedule.CreateScheduleDay(dic, _person2, new DateOnly(2000, 1, 3), currentAuthorization);

            _schedulePart3 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 1), currentAuthorization);
            _schedulePart33 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 2), currentAuthorization);
            _schedulePart333 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 3), currentAuthorization);

            _schedulePart4 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 4), currentAuthorization);
            _schedulePart44 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 4), currentAuthorization);

			_schedulePart5 = ExtractedSchedule.CreateScheduleDay(dic, _person3, new DateOnly(2000, 1, 4), currentAuthorization);
			_schedulePart5.Add(_ass4);

            _schedulePart1.Add(_ass1);
            _schedulePart11.Add(_dayOff1);
            _schedulePart111.Add(_abs1);

            _schedulePart2.Add(_dayOff2);
            _schedulePart22.Add(_ass2);
            _schedulePart222.Add(_abs2);

            _schedulePart333.Add(_ass3);
            _schedulePart333.Add(_abs3);

            _schedulePart4.Add(_dayOff3);
            _schedulePart44.Add(_dayOff3);

            _schedules.Add(_schedulePart1);
            _schedules.Add(_schedulePart11);
            _schedules.Add(_schedulePart111);

            _schedules.Add(_schedulePart2);
            _schedules.Add(_schedulePart22);
            _schedules.Add(_schedulePart222);

            _schedules.Add(_schedulePart3);
            _schedules.Add(_schedulePart33);
            _schedules.Add(_schedulePart333);

            _schedules.Add(_schedulePart4);
            _schedules.Add(_schedulePart44);

			_schedules.Add(_schedulePart5);

        }

        [Test]
        public void CanGetSchedulesWithFreeDays()
        {
            IList<IScheduleDay> schedulesWithFreeDay = ScheduleHelper.SchedulesWithFreeDay(_schedules);

            Assert.IsTrue(schedulesWithFreeDay.Contains(_schedulePart11));
            Assert.IsTrue(schedulesWithFreeDay.Contains(_schedulePart2));
            Assert.IsFalse(schedulesWithFreeDay.Contains(_schedulePart1));
            Assert.IsFalse(schedulesWithFreeDay.Contains(_schedulePart22));
        }
        [Test]
        public void CanGetSchedulesWithSpecificDayOff()
        {
            IList<IScheduleDay> schedulesWithFreeDay = ScheduleHelper.SchedulesWithSpecificDayOff(_schedules, _dayOffTemplate);

            Assert.IsTrue(schedulesWithFreeDay.Contains(_schedulePart4));
            Assert.IsTrue(schedulesWithFreeDay.Contains(_schedulePart44));
            Assert.IsFalse(schedulesWithFreeDay.Contains(_schedulePart11));
            Assert.IsFalse(schedulesWithFreeDay.Contains(_schedulePart22));
        }

		[Test]
		public void ShouldHandleScheduleDayWithOvertimeShift()
		{
			_schedules = new List<IScheduleDay>();
			_schedules.Add(_schedulePart5);
			IList<IScheduleDay> schedulesWithShiftCategory = ScheduleHelper.SchedulesWithShiftCategory(_schedules,
																									   _shiftCategory1);
			Assert.AreEqual(0, schedulesWithShiftCategory.Count);
		}

        [Test]
        public void CanGetSchedulesWithShiftCategory()
        {
            IList<IScheduleDay> schedulesWithShiftCategory = ScheduleHelper.SchedulesWithShiftCategory(_schedules,
                                                                                                        _shiftCategory1);

            Assert.IsTrue(schedulesWithShiftCategory.Contains(_schedulePart1));
            Assert.IsFalse(schedulesWithShiftCategory.Contains(_schedulePart11));
            Assert.IsFalse(schedulesWithShiftCategory.Contains(_schedulePart2));
        }

        [Test]
        public void CanGetSchedulesWithAllShiftCategory()
        {
            IList<IScheduleDay> schedulesWithShiftCategory = ScheduleHelper.SchedulesWithShiftCategory(_schedules);

            Assert.IsTrue(schedulesWithShiftCategory.Contains(_schedulePart1));
            Assert.IsFalse(schedulesWithShiftCategory.Contains(_schedulePart11));
            Assert.IsFalse(schedulesWithShiftCategory.Contains(_schedulePart2));
            Assert.IsTrue(schedulesWithShiftCategory.Contains(_schedulePart22));
        }

        [Test]
        public void CanGetSchedulesWithAbsence()
        {
            IList<IScheduleDay> schedulesWithAbsence = ScheduleHelper.SchedulesWithAbsence(_schedules, _ab1);

            Assert.IsTrue(schedulesWithAbsence.Contains(_schedulePart111));
            Assert.IsFalse(schedulesWithAbsence.Contains(_schedulePart2));

            schedulesWithAbsence = ScheduleHelper.SchedulesWithAbsence(_schedules, _ab2);

            Assert.IsTrue(schedulesWithAbsence.Contains(_schedulePart222));
            Assert.IsFalse(schedulesWithAbsence.Contains(_schedulePart2));
        }
        [Test]
        public void CanGetSchedulesWithAllKindOfAbsence()
        {
            IList<IScheduleDay> schedulesWithAbsence = ScheduleHelper.SchedulesWithAbsence(_schedules);

            Assert.IsTrue(schedulesWithAbsence.Contains(_schedulePart222));
            Assert.IsFalse(schedulesWithAbsence.Contains(_schedulePart2));
            Assert.IsTrue(schedulesWithAbsence.Contains(_schedulePart333));
        }

        [Test]
        public void VerifyContractedTime()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            using(_mocks.Record())
            {
                Expect.Call(schedulePart.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(2));
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(TimeSpan.FromHours(2), ScheduleHelper.ContractedTime(schedulePart));
            }
        }

		[Test]
		public void ShouldReturnSchedulesWithinValidSchedulePeriod()
		{
			var schedulePart1 = _mocks.StrictMock<IScheduleDay>();
			var schedulePart2 = _mocks.StrictMock<IScheduleDay>();
			var person1 = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();
			var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnly = new DateOnly(2013, 1, 1);
			var virtualSchedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var schedules = new List<IScheduleDay> {schedulePart1, schedulePart2};

			using (_mocks.Record())
			{
				Expect.Call(schedulePart1.Person).Return(person1);
				Expect.Call(schedulePart2.Person).Return(person2);
				Expect.Call(schedulePart1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
				Expect.Call(schedulePart2.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
				Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(dateOnly).Repeat.Twice();
				Expect.Call(person1.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod1);
				Expect.Call(person2.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod2);
				Expect.Call(virtualSchedulePeriod1.IsValid).Return(true);
				Expect.Call(virtualSchedulePeriod2.IsValid).Return(false);

			}

			using (_mocks.Playback())
			{
				var result = ScheduleHelper.SchedulesWithinValidSchedulePeriod(schedules);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(schedulePart1, result[0]);
			}	
		}

        #region test help code

        //to get id
        private class ForTestShiftCategory : ShiftCategory
        {
            Guid? _testID;

            public ForTestShiftCategory(string name, string id) : base(name)
            {
                _testID = new Guid(id);
            }

            public override Guid? Id
            {
                get { return _testID; }
            }
        }

        //to get id
        private class ForTestAbsence : Absence
        {
            Guid? _testID;

            public ForTestAbsence(string id)
            {
                _testID = new Guid(id);
            }

            public override Guid? Id
            {
                get { return _testID; }
            }
        }
        private class ForTestDayOffTemplate : DayOffTemplate
        {
            Guid? _testID;

            public ForTestDayOffTemplate(string id)
            {
                _testID = new Guid(id);
                ChangeDescription("Very special template", "VS");
            }

            public override Guid? Id
            {
                get { return _testID; }
            }
        }

        //create personassignment
        private IPersonAssignment CreatePersonAssignment(DateTimePeriod period, IPerson person, IShiftCategory shiftCategory)
        {
            return PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
                                        _scenario, ActivityFactory.CreateActivity("sdfsdf"), period, shiftCategory);
        }

		private IPersonAssignment createPersonAssignmentWithNoMainShiftButOvertime(DateTimePeriod period, IPerson person)
		{
			return PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(person, _scenario, ActivityFactory.CreateActivity("sdfsdf"), period);
		}

        //create persondayoff
        private IPersonAssignment CreatePersonDayOff(DateTime period, IPerson person)
        {
            DateOnly date = new DateOnly(period.Date);

            DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
            dayOff.Anchor = TimeSpan.FromHours(3);
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(35), TimeSpan.FromHours(6));
						return PersonAssignmentFactory.CreateAssignmentWithDayOff(person, _scenario, date, dayOff);
        }
        private IPersonAssignment CreatePersonDayOffWithSpecificTemplate(DateTime period, IPerson person, IDayOffTemplate template)
        {
            DateOnly date = new DateOnly(period.Date);

            template.SetTargetAndFlexibility(TimeSpan.FromHours(35), TimeSpan.FromHours(6));
            template.Anchor = TimeSpan.FromHours(4);
						return PersonAssignmentFactory.CreateAssignmentWithDayOff(person, _scenario, date, template);
        }

        //create personabsence
        private PersonAbsence CreatePersonAbsence(DateTimePeriod period, IPerson person, IAbsence ab)
        {
            return new PersonAbsence(person, _scenario, new AbsenceLayer(ab, period));
        }

        #endregion
    }
}
