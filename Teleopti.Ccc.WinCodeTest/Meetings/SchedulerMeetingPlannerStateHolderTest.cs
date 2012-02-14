using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class SchedulerMeetingPlannerStateHolderTest
    {
        #region Private Members

        private SchedulerMeetingPlannerStateHolder schedulerMeetingPlannerStateHolder;
        private DateTime selectedDate;
        private SchedulerStateHolder schedulerStateHolder;
        private DateTimePeriod sampleDateTimePeriod;
        private IList<IPerson> persons;
        private IPerson person1;
        private IPerson person2;
        private IScheduleDictionary scheduleDictionary;
        private IScenario scenario;
        private DateTime date1;
        private DateTime date2;

        #endregion

        #region Setup and Teardown Methods

        [SetUp]
        public void Setup()
        {

            date1 = new DateTime(2000,01,01);
            date2 = new DateTime(2000, 02, 02);
            sampleDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date1, date2);

            scenario = new Scenario("test");
            persons = new List<IPerson>();
            person1 = new Person();
            person2 = new Person();
            persons.Add(person1);
            persons.Add(person2);

            //object [] arguments = new object[3]{scenario , sampleDateTimePeriod , persons};
            //schedulerStateHolder = mocks.DynamicMock<SchedulerStateHolder>(arguments);
            schedulerStateHolder = new SchedulerStateHolder(scenario, sampleDateTimePeriod, persons);

            schedulerStateHolder.SchedulingResultState = new SchedulingResultStateHolder();

            schedulerMeetingPlannerStateHolder = new SchedulerMeetingPlannerStateHolder(schedulerStateHolder, new List<IPerson>{person1});
            selectedDate = new DateTime(2003,1,1);

            scheduleDictionary = new ScheduleDictionary(scenario,
                                                        new ScheduleDateTimePeriod(sampleDateTimePeriod, persons, 0));
            ((ScheduleRangeLowPermissionCheck)scheduleDictionary[person1]).AddPersonAssignmentRangeUnsafe(new List<IPersonAssignment>());
            ((ScheduleRangeLowPermissionCheck)scheduleDictionary[person2]).AddPersonAssignmentRangeUnsafe(new List<IPersonAssignment>());

            schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
        }

        #endregion

        #region Test Methods

        [Test]
        public void VerifyCanSetSelectedDate()
        {
            schedulerMeetingPlannerStateHolder.SelectedDate = selectedDate;

            Assert.AreEqual(schedulerMeetingPlannerStateHolder.SelectedDate, selectedDate);

        }

        [Test]
        public void VerifyCanLoadAllPersons()
        {
            Assert.AreEqual(2, schedulerMeetingPlannerStateHolder.AllPersonCollection.Count);
        }

        [Test]
        public void VerifyCanLoadSchedules()
        {
            IList<IPerson> personsToLoad = new List<IPerson>();
            personsToLoad.Add(person1);
            schedulerMeetingPlannerStateHolder.LoadSchedules(personsToLoad, sampleDateTimePeriod, scenario,null);

            Assert.AreEqual(1,schedulerMeetingPlannerStateHolder.ScheduleDictionary.Count);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCannotLoadSchedulesFromOtherScenario()
        {
            schedulerMeetingPlannerStateHolder.LoadSchedules(persons, sampleDateTimePeriod, new Scenario("other"),null);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCannotLoadSchedulesWithOtherPersons()
        {
            IList<IPerson> personsToLoad = new List<IPerson>();
            personsToLoad.Add(new Person());
            schedulerMeetingPlannerStateHolder.LoadSchedules(personsToLoad, sampleDateTimePeriod, scenario,null);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCannotLoadSchedulesWithNotContainedPeriod()
        {
            schedulerMeetingPlannerStateHolder.LoadSchedules(persons, sampleDateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1)), scenario,null);
        }

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void VerifyCanLoadSelectedMeetingPersons()
        //{
        //    schedularMeetingHolder.LoadSelectedPersons();

        //    Assert.IsNull(schedularMeetingHolder.SelectedPersonCollection);
        //}

        [Test]
        public void VerifyCanLoadAllScenarios()
        {
            schedulerMeetingPlannerStateHolder.LoadAllScenarios();

            Assert.AreEqual(1, schedulerMeetingPlannerStateHolder.ScenarioCollection.Count);
            Assert.AreSame(scenario, schedulerMeetingPlannerStateHolder.ScenarioCollection[0]);
        }


        [Test]
        public void VerifyCanGetSelectedPersons()
        {
            Assert.AreEqual(1, schedulerMeetingPlannerStateHolder.SelectedPersonCollection.Count);
            Assert.AreSame(person1, schedulerMeetingPlannerStateHolder.SelectedPersonCollection[0]);
        }

        //[Test]
        //public void VerifyCanGetAllMeetings()
        //{
        //    Assert.IsNull(schedularMeetingHolder.MeetingCollection);
        //}

      
        #endregion

    }                                 
}
