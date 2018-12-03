using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
	public class MeetingSlotFinderServiceTest
    {
        private MeetingSlotFinderService _target;
        private IPerson _person1;
        private IPerson _person2;
        private IList<DateOnly> _dates;
        private IList<IPerson> _persons;
        private MockRepository _mocks;
        private IScheduleDictionary _dictionary;
        private IScenario _scenario;
        private DateTimePeriod _d1;
        private DateTimePeriod _d2;
        private DateTimePeriod _d3;
        private IScheduleDay _p1D1;
        private IScheduleDay _p1D2;
        private IScheduleDay _p1D3;
        private IScheduleDay _p2D1;
        private IScheduleDay _p2D2;
        private IScheduleDay _p2D3;
        private IScheduleRange _range1;
        private IScheduleRange _range2;
        private TimeSpan _duration;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        private DateTimePeriod _periodToNextDay;
        private IScheduleDay _scheduleDayToNextDay;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person1 = new Person();
            _person2 = new Person();
            _dates = new List<DateOnly>();
            _persons = new List<IPerson>();
            _target = new MeetingSlotFinderService(new SpecificTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()));
            _dictionary = _mocks.StrictMock<IScheduleDictionary>();
            _range1 = _mocks.StrictMock<IScheduleRange>();
            _range2 = _mocks.StrictMock<IScheduleRange>();
            _d1 = new DateTimePeriod(new DateTime(2009, 2, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 1, 15, 15, 0, DateTimeKind.Utc));
            _d2 = new DateTimePeriod(new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 3, 0, 0, 0, DateTimeKind.Utc));
            _d3 = new DateTimePeriod(new DateTime(2009, 2, 3, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 4, 0, 0, 0, DateTimeKind.Utc));
            _periodToNextDay = new DateTimePeriod(new DateTime(2009, 2, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 2, 0, 0, DateTimeKind.Utc));
            _duration = new TimeSpan(1, 0, 0);
            _startTime = new TimeSpan(8, 0, 0);
            _endTime = new TimeSpan(17, 0, 0);
            _scenario = new Scenario("hej");
            IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("hej");
            IActivity activity = ActivityFactory.CreateActivity("hej");
            activity.InWorkTime = true;
            activity.AllowOverwrite = true;
            var absencePeriod1 = new DateTimePeriod(_d1.StartDateTime.AddHours(1), _d1.EndDateTime.Subtract(TimeSpan.FromHours(1)));
            var absencePeriod2 = new DateTimePeriod(_d2.StartDateTime.AddHours(10), _d2.EndDateTime.Subtract(TimeSpan.FromHours(10)));
            var absencePeriod3 = new DateTimePeriod(_d3.StartDateTime.AddHours(12), _d3.EndDateTime.Subtract(TimeSpan.FromHours(10)));

            var theFirstDate = new DateOnly(2009, 02, 03);
            
            IDayOffTemplate dOff = DayOffFactory.CreateDayOff();
            dOff.Anchor = TimeSpan.FromHours(13);

            using (_mocks.Record())
            {
                Expect.Call(_dictionary.PermissionsEnabled).Return(true).Repeat.Any();
                Expect.Call(_dictionary.Scenario).Return(_scenario).Repeat.Any();
            }
            using (_mocks.Playback())
            {
				var currentAuthorization = CurrentAuthorization.Make();
				_p1D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2009, 2, 1), currentAuthorization);

                _p1D1.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d1, category));
                _p1D1.Add(PersonAbsenceFactory.CreatePersonAbsence(_person1, _scenario, absencePeriod1));

                _p2D1 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2009, 2, 1), currentAuthorization);
				_p2D1.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d1, category));

                _p1D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2009, 2, 2), currentAuthorization);
				_p1D2.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d2, category));

                _p2D2 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2009, 2, 2), currentAuthorization);
				_p2D2.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d2, category));
                _p2D2.Add(PersonAbsenceFactory.CreatePersonAbsence(_person2, _scenario, absencePeriod2));


                _p1D3 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2009, 2, 3), currentAuthorization);
								_p1D3.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person1, _scenario, theFirstDate, dOff));

                _p2D3 = ExtractedSchedule.CreateScheduleDay(_dictionary, _person2, new DateOnly(2009, 2, 3), currentAuthorization);
				_p2D3.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _d3, category));
                _p2D3.Add(PersonAbsenceFactory.CreatePersonAbsence(_person2, _scenario, absencePeriod3));

                _scheduleDayToNextDay = ExtractedSchedule.CreateScheduleDay(_dictionary, _person1, new DateOnly(2009, 2, 1), currentAuthorization);
				_scheduleDayToNextDay.AddMainShift(EditableShiftFactory.CreateEditorShift(activity, _periodToNextDay, category));
                _scheduleDayToNextDay.Add(PersonAbsenceFactory.CreatePersonAbsence(_person1, _scenario, _periodToNextDay));
            }
            _mocks.BackToRecordAll();
        }

        [Test]
        public void VerifyCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyDatesCannotBeEmpty()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.FindSlots(_dates[0], _duration, _startTime, _endTime, _dictionary, _persons));
        }

        [Test]
        public void VerifyDictionaryCannotBeNull()
        {
            _dates.Add(new DateOnly(2009, 02, 02));
			Assert.Throws<ArgumentNullException>(() => _target.FindSlots(_dates[0], _duration, _startTime, _endTime, null, _persons));
        }

        [Test]
        public void VerifyCheckAbsenceIntersect()
        {
            _dates.Add(new DateOnly(2009, 02, 01));
            _persons.Add(_person1);
            _persons.Add(_person2);
            
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_dictionary[_persons[1]]).Return(_range2).Repeat.Times(1);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindSlots(_dates[0], _duration, _startTime, _endTime, _dictionary, _persons).Count, 1);
            }
        }

        [Test]
        public void VerifyCheckEndTimeNextDay()
        {
            var dur = new TimeSpan(28, 0, 0);
            _dates.Add(new DateOnly(2009, 02, 01));
            _persons.Add(_person1);
            _persons.Add(_person2);
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_dictionary[_persons[1]]).Return(_range2).Repeat.Times(1);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindSlots(_dates[0], dur, _startTime, _endTime, _dictionary, _persons).Count, 0);
            }
        }

        [Test]
        public void ShouldHandleAbsencesAndActivitiesEndingNextDay()
        {
            var dur = new TimeSpan(28, 0, 0);
            _dates.Add(new DateOnly(2009, 02, 01));
            _persons.Add(_person1);
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_scheduleDayToNextDay);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindSlots(_dates[0], dur, _startTime, _endTime, _dictionary, _persons).Count, 0);
            }    
        }
            
        [Test]
        public void VerifyCheckAbsenceIntersectWithRoundDown()
        {
            _dates.Add(new DateOnly(2009, 02, 01));
            _persons.Add(_person1);
            _persons.Add(_person2);
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_dictionary[_persons[1]]).Return(_range2).Repeat.Times(1);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindSlots(_dates[0], _duration, _startTime, _endTime, _dictionary, _persons).Count, 1);
            }
        }

        [Test]
        public void VerifyCheckDayOff()
        {
            _dates.Add(new DateOnly(2009, 02, 03));
            _persons.Add(_person1);
            _persons.Add(_person2);
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D3);
                Expect.Call(_dictionary[_persons[1]]).Return(_range2).Repeat.Times(1);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D3);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindSlots(_dates[0], _duration, _startTime, _endTime, _dictionary, _persons).Count, 0);
            }
        }

        [Test]
        public void VerifyFindSlots()
        {
            _dates.Add(new DateOnly(2009, 02, 01));
            _dates.Add(new DateOnly(2009, 02, 02));
            _dates.Add(new DateOnly(2009, 02, 03));
            _persons.Add(_person1);
            _persons.Add(_person2);
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(1);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_dictionary[_persons[1]]).Return(_range2).Repeat.Times(1);
                Expect.Call(_range2.ScheduledDay(_dates[0])).Return(_p2D1);
            }
            using (_mocks.Playback())
            {
                _target.FindSlots(_dates[0], _duration, _startTime, _endTime, _dictionary, _persons);
            }
        }

        [Test]
        public void VerifyFindAvailableDays()
        {
            _dates.Add(new DateOnly(2009, 02, 01));
            _dates.Add(new DateOnly(2009, 02, 02));
            _persons.Add(_person1);
            IList<DateOnly> available = new List<DateOnly> {new DateOnly(2009, 02, 01), new DateOnly(2009, 02, 02)};
            using (_mocks.Record())
            {
                Expect.Call(_dictionary[_persons[0]]).Return(_range1).Repeat.Times(2);
                Expect.Call(_range1.ScheduledDay(_dates[0])).Return(_p1D1);
                Expect.Call(_range1.ScheduledDay(_dates[1])).Return(_p1D2);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.FindAvailableDays(_dates, _duration, _startTime, _endTime, _dictionary, _persons).Count, available.Count);
            }
        }

        [Test]
        public void ShouldNotReturnSlotOutsideWorkTime()
        {
            var wholePeriod = new DateTimePeriod(new DateTime(2009, 2, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 1, 17, 0, 0, DateTimeKind.Utc));
            var lunch = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 3, 13, 0, 0, DateTimeKind.Utc));
            
            var dateOnly = new DateOnly(2011, 3, 9);
            var day = _mocks.StrictMock<IScheduleDay>();
            var projService = _mocks.StrictMock<IProjectionService>();

            var lunchActivity = ActivityFactory.CreateActivity("lunch");
            lunchActivity.AllowOverwrite = true;

            var activity = ActivityFactory.CreateActivity("hej");
            activity.InWorkTime = true;
            activity.AllowOverwrite = true;
            var layerBefore = new VisualLayer(activity, wholePeriod,
                                activity);
            var layerLunch =
                new VisualLayer(lunchActivity, lunch,
                                lunchActivity);
            var layerAfter = new VisualLayer(activity , wholePeriod ,
                                activity); 

            var layers = new List<IVisualLayer> { layerBefore , layerLunch, layerAfter};
            var layerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredLayerCollection = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            var personAss = _mocks.StrictMock<IPersonAssignment>();
            Expect.Call(_dictionary[_person1]).Return(_range1);
            Expect.Call(_range1.ScheduledDay(dateOnly)).Return(day);
            Expect.Call(day.HasDayOff()).Return(false);
            Expect.Call(day.PersonAssignment()).Return(personAss).Repeat.Twice();
            Expect.Call(day.ProjectionService()).Return(projService);
            Expect.Call(projService.CreateProjection()).Return(layerCollection);
        	Expect.Call(layerCollection.FilterLayers<IAbsence>()).Return(filteredLayerCollection);
        	Expect.Call(filteredLayerCollection.GetEnumerator()).Return(new List<IVisualLayer>{layerLunch}.GetEnumerator());
        	Expect.Call(layerCollection.GetEnumerator()).Return(layers.GetEnumerator());
            Expect.Call(personAss.Period).Return(wholePeriod);
           
            _mocks.ReplayAll();
            var result = _target.FindSlots(dateOnly, _duration, _startTime, _endTime, _dictionary, new List<IPerson>{_person1});
            Assert.That(result.Count, Is.EqualTo(7));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotReturnSlotOutsideWorkTimeNotAllowMeeting()
        {
            var wholePeriod = new DateTimePeriod(new DateTime(2009, 2, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 1, 17, 0, 0, DateTimeKind.Utc));
            var lunch = new DateTimePeriod(new DateTime(2009, 2, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 1, 14, 0, 0, DateTimeKind.Utc));

            var dateOnly = new DateOnly(2009, 2, 1);
            var day = _mocks.StrictMock<IScheduleDay>();
            var projService = _mocks.StrictMock<IProjectionService>();

            var lunchActivity = ActivityFactory.CreateActivity("lunch");
            lunchActivity.AllowOverwrite = false  ;

            var activity = ActivityFactory.CreateActivity("hej");
            activity.InWorkTime = true;
            activity.AllowOverwrite = true ;
            var layerBefore = new VisualLayer(activity, wholePeriod,
                                activity);
            var layerLunch =
                new VisualLayer(lunchActivity, lunch,
                                lunchActivity);
            var layerAfter = new VisualLayer(activity, wholePeriod,
                                activity);

            var layers = new List<IVisualLayer> { layerBefore, layerLunch, layerAfter };
            var layerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredLayerCollection = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            var personAss = _mocks.StrictMock<IPersonAssignment>();
            Expect.Call(_dictionary[_person1]).Return(_range1);
            Expect.Call(_range1.ScheduledDay(dateOnly)).Return(day);
            Expect.Call(day.HasDayOff()).Return(false);
            Expect.Call(day.PersonAssignment()).Return(personAss).Repeat.Twice();
            Expect.Call(day.ProjectionService()).Return(projService);
            Expect.Call(projService.CreateProjection()).Return(layerCollection);
            Expect.Call(layerCollection.FilterLayers<IAbsence>()).Return(filteredLayerCollection);
            Expect.Call(filteredLayerCollection.GetEnumerator()).Return(new List<IVisualLayer> {  }.GetEnumerator());
            Expect.Call(layerCollection.GetEnumerator()).Return(layers.GetEnumerator());
            Expect.Call(personAss.Period).Return(wholePeriod);

            _mocks.ReplayAll();
            var result = _target.FindSlots(dateOnly, _duration, _startTime, _endTime, _dictionary, new List<IPerson> { _person1 });
            Assert.That(result.Count, Is.EqualTo(10));
            _mocks.VerifyAll();
        }
       
    }
}
