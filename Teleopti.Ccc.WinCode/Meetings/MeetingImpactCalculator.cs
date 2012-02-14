using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public interface IMeetingImpactCalculator
    {
        void RecalculateResources(DateOnly dateToCalculate);
        void RemoveAndRecalculateResources(IMeeting meeting, DateOnly dateToCalculate);
    }

    public class MeetingImpactCalculator : IMeetingImpactCalculator
    {
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IMeeting _meeting;
        private readonly ISchedulerStateHolder _schedulerStateHolder;

        public MeetingImpactCalculator(ISchedulerStateHolder schedulerStateHolder, IResourceOptimizationHelper resourceOptimizationHelper, IMeeting meeting)
        {
            _schedulerStateHolder = schedulerStateHolder;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _meeting = meeting;
        }

        public void RecalculateResources(DateOnly dateToCalculate)
        {
            RemoveMeetingFromDictionary(_meeting);
            AddPersonMeetingsToDictionary(_meeting);

            _resourceOptimizationHelper.ResourceCalculateDate(dateToCalculate, false, true);
        }

        public void RemoveAndRecalculateResources(IMeeting meeting, DateOnly dateToCalculate)
        {
            RemoveMeetingFromDictionary(_meeting);

            _resourceOptimizationHelper.ResourceCalculateDate(dateToCalculate, false, true);
        }

        private void RemoveMeetingFromDictionary(IMeeting meeting)
        {
            foreach (var meetingPerson in meeting.MeetingPersons)
            {
                foreach (var personMeeting in GetPersonMeetingsFromMeeting(meeting))
                {
                    ((ScheduleRange)_schedulerStateHolder.Schedules[meetingPerson.Person]).Remove(personMeeting);
                }
            }
        }

        private void AddPersonMeetingsToDictionary(IMeeting meeting)
        {
            foreach (var meetingPerson in meeting.MeetingPersons)
            {
                var person = meetingPerson.Person;
                var personMeetings = meeting.GetPersonMeetings(person);
                if (personMeetings.Count > 0)
                    ((ScheduleRange)_schedulerStateHolder.Schedules[person]).Add(personMeetings[0]);
                // we can only look at the first if recurrent, otherwise we might come outside the loaded period
                //foreach (var personMeeting in personMeetings)
                //{
                //    ((ScheduleRange)_schedulerStateHolder.Schedules[person]).Add(personMeeting);
                //}
            }
        }


        private IEnumerable<IPersonMeeting> GetPersonMeetingsFromMeeting(IMeeting meeting)
        {
            IList<IPersonMeeting> personMeetings = new List<IPersonMeeting>();

            var scheduleDays = _schedulerStateHolder.Schedules.SchedulesForDay(meeting.StartDate);

            foreach (var scheduleDay in scheduleDays)
            {
                foreach (var personMeeting in scheduleDay.PersonMeetingCollection())
                {
                    if (personMeeting.BelongsToMeeting == meeting)
                        personMeetings.Add(personMeeting);
                }
            }

            return personMeetings;
        }
    }
}