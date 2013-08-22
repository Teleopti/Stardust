using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.AuditHistory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AuditHistory
{
    [TestFixture]
    public class AuditHistoryScheduleDayCreatorTest
    {
        private IAuditHistoryScheduleDayCreator _target;
        private IScheduleDay _currentScheduleDay;
        private IPerson _person;

        private IScheduleParameters _parameters;
        private IScheduleRange _range;
        private DateTimePeriod _rangePeriod;
        private IPersonAbsence _abs;
        private IPersonAssignment _ass1;
        private IPersonMeeting _personMeeting;
        private IScenario _scenario;
        private IScheduleDictionary _dic;
        private MockRepository _mocks;
        private IDictionary<IPerson, IScheduleRange> _underlyingDictionary;
        private INote _note;
        private IList<IPersistableScheduleData> _newData;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SetUp]
        public void Setup()
        {
            _target = new AuditHistoryScheduleDayCreator();
            _mocks = new MockRepository();
            _person = PersonFactory.CreatePerson();

			_range = _mocks.StrictMock<IScheduleRange>();
			_rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
			_parameters =
				new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), _person, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			_scenario = _parameters.Scenario;
			_underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			_dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_rangePeriod), _underlyingDictionary);
            _currentScheduleDay = ExtractedSchedule.CreateScheduleDay(_dic, _parameters.Person, new DateOnly(_parameters.Period.StartDateTime));
			_underlyingDictionary.Add(_parameters.Person, _range);
			_note = new Note(_parameters.Person, new DateOnly(2000, 1, 1), _scenario, "The agent is very cute");
			
			_abs =
				PersonAbsenceFactory.CreatePersonAbsence(_parameters.Person, _parameters.Scenario,
														 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            _currentScheduleDay.Add(_abs);

			_ass1	 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_parameters.Scenario, _parameters.Person,
																	  _parameters.Period);

			IMeeting meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description",
				ActivityFactory.CreateActivity("activity"), _parameters.Scenario);

			_personMeeting = new PersonMeeting(meeting, new MeetingPerson(_parameters.Person, true), _rangePeriod);


			_personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(_parameters.Person, true));

            _currentScheduleDay.Add(_personMeeting);
            _currentScheduleDay.Add(_ass1);
            _currentScheduleDay.Add(_note);

			DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
			dayOff.Anchor = TimeSpan.FromHours(12);
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            _newData = new List<IPersistableScheduleData>();
            _newData.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_parameters.Scenario, _parameters.Person,_parameters.Period));	
            _newData.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _parameters.Period));
            _newData.Add(PersonDayOffFactory.CreatePersonDayOff(_person, _scenario, new DateOnly(2000, 1, 1), new DayOffTemplate(new Description("Hej"))));
        }

        [Test]
        public void ShouldEmptyPersonAssignments()
        {
            using(_mocks.Record())
            {
                
            }
            IScheduleDay result;
            using(_mocks.Playback())
            {
                result = _target.Create(_currentScheduleDay, new List<IPersistableScheduleData>());
            }

	        result.PersonAssignment().Should().Be.Null();
					_currentScheduleDay.PersonAssignment().Should().Not.Be.Null();
        }

        [Test]
        public void ShouldEmptyPersonAbsences()
        {
            using (_mocks.Record())
            {

            }
            IScheduleDay result;
            using (_mocks.Playback())
            {
                result = _target.Create(_currentScheduleDay, new List<IPersistableScheduleData>());
            }

            Assert.AreEqual(0, result.PersonAbsenceCollection().Count);
            Assert.AreEqual(1, _currentScheduleDay.PersonAbsenceCollection().Count);
        }


        [Test]
        public void ShouldAddNewPersonAssignments()
        {
            using (_mocks.Record())
            {

            }
            IScheduleDay result;
            using (_mocks.Playback())
            {
                result = _target.Create(_currentScheduleDay, _newData);
            }

						result.PersonAssignment().Should().Not.Be.Null();
        }

		[Test]
		public void ShouldAddNewPersonAbsencesSpanOverSeveralDays()
		{
			_parameters = new ScheduleParameters(_scenario, _person, new DateTimePeriod(2000, 1, 1, 2001, 1, 3));
			var newData = new List<IPersistableScheduleData>();
			newData.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _parameters.Period));
			var abs = PersonAbsenceFactory.CreatePersonAbsence(_parameters.Person, _parameters.Scenario,
													 new DateTimePeriod(2000, 1, 1, 2001, 1, 3));
			var currentScheduleDay = ExtractedSchedule.CreateScheduleDay(_dic, _parameters.Person, new DateOnly(2000, 1, 2));
			currentScheduleDay.Add(abs);

			var result = _target.Create(currentScheduleDay, newData);

			Assert.AreEqual(1, result.PersonAbsenceCollection().Count);
			
			currentScheduleDay = ExtractedSchedule.CreateScheduleDay(_dic, _parameters.Person, new DateOnly(2000, 1, 3));
			currentScheduleDay.Add(abs);

			result = _target.Create(currentScheduleDay, newData);

			Assert.AreEqual(1, result.PersonAbsenceCollection().Count);
		}

        [Test]
        public void ShouldAddNewAbsences()
        {
            using (_mocks.Record())
            {

            }
            IScheduleDay result;
            using (_mocks.Playback())
            {
                result = _target.Create(_currentScheduleDay, _newData);
            }

            Assert.AreEqual(1, result.PersonAbsenceCollection().Count);
        }


		[Test]
		public void ShouldSkipDataStartingOutsideCurrentDay()
		{
			_newData.Clear();
			_newData.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_parameters.Scenario, _parameters.Person, _parameters.Period.ChangeStartTime(TimeSpan.FromDays(-1))));

			var result = _target.Create(_currentScheduleDay, _newData);
			result.PersonAssignment().Should().Be.Null();
		}	
    }
}