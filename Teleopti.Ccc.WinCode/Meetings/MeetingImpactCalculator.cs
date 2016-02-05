using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
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

            _resourceOptimizationHelper.ResourceCalculateDate(dateToCalculate, true, false);
        }

        public void RemoveAndRecalculateResources(IMeeting meeting, DateOnly dateToCalculate)
        {
            RemoveMeetingFromDictionary(_meeting);

            _resourceOptimizationHelper.ResourceCalculateDate(dateToCalculate, true, false);
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
        	var loadedPeriod = _schedulerStateHolder.Schedules.Period.LoadedPeriod();
			using (TurnoffPermissionScope.For((IPermissionCheck) _schedulerStateHolder.Schedules))
        	{
				foreach (var meetingPerson in meeting.MeetingPersons)
				{
					var person = meetingPerson.Person;
					var personMeetings = meeting.GetPersonMeetings(person).Where(m => loadedPeriod.Contains(m.Period.StartDateTime));
					foreach (var personMeeting in personMeetings)
					{
						((ScheduleRange)_schedulerStateHolder.Schedules[person]).Add(personMeeting);
						break;
					}
				}        		
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