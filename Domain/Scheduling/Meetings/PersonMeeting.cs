using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// class for meeting to be used in schedule
    /// </summary>
    public class PersonMeeting : IPersonMeeting
    {
        private readonly IMeetingPerson _meetingPerson;
        private readonly IMeeting _belongsToMeeting;
        private readonly DateTimePeriod _period;

        /// <summary>
        /// Meeting to be used in schedules
        /// </summary>
        /// <param name="meeting">The meeting.</param>
        /// <param name="meetingPerson">The meeting person.</param>
        /// <param name="period">The period.</param>
        public PersonMeeting(IMeeting meeting, IMeetingPerson meetingPerson, DateTimePeriod period)
        {
            _period = period;
            _meetingPerson = meetingPerson;
            _belongsToMeeting = meeting;
        }

        /// <summary>
        /// Optional
        /// </summary>
        public Boolean Optional => _meetingPerson.Optional;

		/// <summary>
        /// Meeting this personMeeting belongs to
        /// </summary>
        public IMeeting BelongsToMeeting => _belongsToMeeting;

		public ILayer<IActivity> ToLayer()
        {
            return new MeetingLayer(_belongsToMeeting.Activity, _period);
        }

        /// <summary>
        /// Person
        /// </summary>
        public IPerson Person => _meetingPerson.Person;

		/// <summary>
        /// Scenario
        /// </summary>
        public virtual IScenario Scenario => _belongsToMeeting.Scenario;

		/// <summary>
        /// Period
        /// </summary>
        public DateTimePeriod Period => _period;

		/// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return EntityClone();
        }

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
			var addExtraEndDayDueToNightShifts = dateAndPeriod.Period().ChangeEndTime(TimeSpan.FromDays(1));
			return addExtraEndDayDueToNightShifts.Contains(_period.StartDateTime);
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            DateTimePeriod dateTimePeriod =
                dateOnlyPeriod.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone());

            var addExtraEndDayDueToNightShifts = dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1));
            return addExtraEndDayDueToNightShifts.Contains(Period.StartDateTime);
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return Scenario.Equals(scenario);
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        public IPersonMeeting NoneEntityClone()
        {
            PersonMeeting retobj = (PersonMeeting)MemberwiseClone();
           
            return retobj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public IPersonMeeting EntityClone()
        {
            PersonMeeting retobj = (PersonMeeting)MemberwiseClone();
           
            return retobj;
        }

		public override int GetHashCode()
		{
			return Person.GetHashCode() ^ BelongsToMeeting.GetHashCode() ^ Period.GetHashCode();
		}
	}
}
