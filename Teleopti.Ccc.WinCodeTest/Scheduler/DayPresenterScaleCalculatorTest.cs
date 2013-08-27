using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DayPresenterScaleCalculatorTest
    {

        private MockRepository _mocks;
        private ISchedulerStateHolder _stateHolder;
        private IDayPresenterScaleCalculator _target;
        private IDictionary<Guid, IPerson> _persons;
        private IPerson _person;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleRange _range;
        private IScheduleDay _scheduleDay1;
	    private IEditableShiftMapper _editableShiftMapper;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
    		_editableShiftMapper = _mocks.StrictMock<IEditableShiftMapper>();
            _target = new DayPresenterScaleCalculator(_editableShiftMapper);
            _person = PersonFactory.CreatePerson();
            _persons = new Dictionary<Guid, IPerson>();
            _persons.Add(Guid.NewGuid(), _person);
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _range = _mocks.StrictMock<IScheduleRange>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			TimeZoneGuard.Instance.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
        }

        [Test]
        public void ScalePeriodShouldAddOneHourToBeginningAndEnd()
        {
            DateTimePeriod assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod1);
            DateTimePeriod assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass1);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass1)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod1, new ShiftCategory("hopp")));
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass2);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass2)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod2, new ShiftCategory("hopp")));
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            DateTimePeriod expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.StartDateTime, result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldStartAtZeroIfNightshiftDayBefore()
        {
            DateTimePeriod assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod1);
            DateTimePeriod assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass1);
	            Expect.Call(_editableShiftMapper.CreateEditorShift(ass1)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod1, new ShiftCategory("hopp")));
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass2);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass2)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod2, new ShiftCategory("hopp")));
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            DateTimePeriod expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithNoAssignmentAndNightshiftDayBefore()
        {
            DateTimePeriod assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod1);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass1);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass1)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod1, new ShiftCategory("hopp")));
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay1);
	            Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            DateTimePeriod expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), result.StartDateTime);
            Assert.AreEqual(expected.LocalEndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithNoAssignmentAndNoNightshiftDayBefore()
        {
            DateTimePeriod assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod1);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass1);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass1)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod1, new ShiftCategory("hopp")));
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay1);
	            Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            DateTimePeriod expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.LocalStartDateTime, result.StartDateTime);
            Assert.AreEqual(expected.LocalEndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithOnlyNightshift()
        {
            DateTimePeriod assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod1);
            DateTimePeriod assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 3, 6, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assPeriod2);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass1);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass1)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod1, new ShiftCategory("hopp")));
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(ass2);
				Expect.Call(_editableShiftMapper.CreateEditorShift(ass2)).Return(EditableShiftFactory.CreateEditorShift(new Activity("hej"), assPeriod2, new ShiftCategory("hopp")));
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            DateTimePeriod expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 3, 7, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.StartDateTime, result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }
    }
}