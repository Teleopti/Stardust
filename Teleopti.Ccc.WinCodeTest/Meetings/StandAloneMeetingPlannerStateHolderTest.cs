using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class StandAloneMeetingPlannerStateHolderTest
    {
        #region Private Members

        private MockRepository mocks;

        private StandAloneMeetingPlannerStateHolder standAloneMeetingPlannerStateHolder;

        private DateTime selectedDate;

        private DateTimePeriod sampleDateTimePeriod;

        private IScheduleRepository _scheduleRepository;
        
        private IMeetingRepository _meetingRepository;
        
        private IScenarioRepository _scenarioRepository;
        
        private DateTime date1;
        
        private DateTime date2;

        #endregion

        #region Setup and Teardown Methods

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

            date1 = new DateTime(2000, 01, 01);
            date2 = new DateTime(2000, 02, 02);

            _scheduleRepository = mocks.CreateMock<IScheduleRepository>();
            _meetingRepository = mocks.CreateMock<IMeetingRepository>();
            _scenarioRepository = mocks.CreateMock<IScenarioRepository>();

            standAloneMeetingPlannerStateHolder = new StandAloneMeetingPlannerStateHolder(_scheduleRepository, _scenarioRepository);
            selectedDate = new DateTime(2003, 3, 3);
            sampleDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date1, date2);

        }

        #endregion

        #region Test Methods

        [Test]
        public void VerifyCanSetSelectedDate()
        {
            standAloneMeetingPlannerStateHolder.SelectedDate = selectedDate;

            Assert.AreEqual(standAloneMeetingPlannerStateHolder.SelectedDate, selectedDate);

        }

        [Test]
        public void VerifyCanLoadAllMeetingPersons()
        {
            ICollection<IPerson> people = new List<IPerson>();
            IPerson per = new Person();
            people.Add(per);

            standAloneMeetingPlannerStateHolder.SetAllPersons(people);
            Assert.AreEqual(1, standAloneMeetingPlannerStateHolder.AllPersonCollection.Count);
        }

        [Test]
        public void VerifyCanLoadSchedules()
        {
            standAloneMeetingPlannerStateHolder.LoadSchedules(null, sampleDateTimePeriod, null, null);

            Assert.IsNull(standAloneMeetingPlannerStateHolder.ScheduleDictionary);
        }

        [Test]
        public void VerifyCanLoadSelectedMeetingPersons()
        {
            //            //TODO : Need to figureout what needs to be done in here for the stand alone meeting planner stateholder
            //            IList<IPerson> persons = mocks.CreateMock<IList<IPerson>>();
            //            IPerson person1 = mocks.CreateMock<IPerson>();
            //            IPerson person2 = mocks.CreateMock<IPerson>();
            //            persons.Add(person1);
            //            persons.Add(person2);

            //#pragma warning disable 0618
            //            Expect.Call(_personRepository.LoadAll()).IgnoreArguments().Return(persons);
            //#pragma warning restore 0618

            //            mocks.ReplayAll();

            //            standAloneMeetingPlannerStateHolder.LoadSelectedPersons();

            Assert.AreEqual(standAloneMeetingPlannerStateHolder.SelectedPersonCollection, null);
        }

        [Test]
        public void VerifyCanLoadAllScenarios()
        {
            IList<IScenario> scenarios = mocks.CreateMock<IList<IScenario>>();
            IScenario scenario1 = mocks.CreateMock<IScenario>();
            IScenario scenario2 = mocks.CreateMock<IScenario>();
            scenarios.Add(scenario1);
            scenarios.Add(scenario2);

            Expect.Call(_scenarioRepository.FindAllSorted()).IgnoreArguments().Return(scenarios);

            mocks.ReplayAll();

            standAloneMeetingPlannerStateHolder.LoadAllScenarios();

            Assert.IsNotNull(standAloneMeetingPlannerStateHolder.ScenarioCollection);
        }


        [Test]
        public void VerifyCanLoadAllMeetings()
        {
            IList<IMeeting> meetings = mocks.CreateMock<IList<IMeeting>>();
            IMeeting meeting1 = mocks.CreateMock<IMeeting>();
            IMeeting meeting2 = mocks.CreateMock<IMeeting>();
            meetings.Add(meeting1);
            meetings.Add(meeting2);

            //using (mocks.Record())
            //{
#pragma warning disable 0618
                Expect.Call(_meetingRepository.Find(null, sampleDateTimePeriod, null)).IgnoreArguments().Return(
                    meetings);
#pragma warning restore 0618
            //}
                mocks.ReplayAll();
            //using (mocks.Playback())
            //{
                standAloneMeetingPlannerStateHolder.LoadMeetings(null, sampleDateTimePeriod, null, _meetingRepository);

                Assert.IsNotNull(standAloneMeetingPlannerStateHolder.MeetingCollection);
            //}
        }

        [Test]
        public void VerifyCanAddMeeting()
        {
            IMeetingRepository meetingRepository = mocks.CreateMock<IMeetingRepository>();
            IMeeting meeting = mocks.CreateMock<IMeeting>();
            standAloneMeetingPlannerStateHolder.AddMeeting(meetingRepository, meeting);
        }

        [Test]
        public void VerifyCanRemoveMeeting()
        {
            IMeetingRepository meetingRepository = mocks.CreateMock<IMeetingRepository>();
            IMeeting meeting = mocks.CreateMock<IMeeting>();
            standAloneMeetingPlannerStateHolder.RemoveMeeting(meetingRepository, meeting);
        }

        #endregion
    }
}
