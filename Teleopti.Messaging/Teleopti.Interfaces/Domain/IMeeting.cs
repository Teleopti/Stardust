﻿using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Meeting
    /// </summary>
    public interface IMeeting : IAggregateRoot, ICloneableEntity<IMeeting>, IChangeInfo, IBelongsToBusinessUnit
    {
        /// <summary>
        /// Activity
        /// </summary>
        IActivity Activity { get; set; }

        /// <summary>
        /// AddMeetingPerson
        /// </summary>
        /// <param name="meetingPerson"></param>
        void AddMeetingPerson(IMeetingPerson meetingPerson);

        /// <summary>
        /// Clear MeetingPersons
        /// </summary>
        void ClearMeetingPersons();

        /// <summary>
        /// ContainsPerson
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        bool ContainsPerson(IPerson person);

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// MeetingPersons
        /// </summary>
		IEnumerable<IMeetingPerson> MeetingPersons { get; }

        /// <summary>
        /// Organizer
        /// </summary>
        IPerson Organizer { get; set; }

        /// <summary>
        /// PersonMeeting
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        IList<IPersonMeeting> GetPersonMeetings(IPerson person);

        /// <summary>
        /// Remove person
        /// </summary>
        /// <param name="person"></param>
        void RemovePerson(IPerson person);

        /// <summary>
        /// Scenario
        /// </summary>
        IScenario Scenario { get; }

        /// <summary>
        /// Subject
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 1/9/2009
        /// </remarks>
        TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 1/9/2009
        /// </remarks>
        TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        ICccTimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the end date for recurrencies. Cannot be less than start date.
        /// </summary>
        /// <value>The end date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        DateOnly EndDate { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets or sets the meeting recurrences.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/8/2008
        /// </remarks>
        IRecurrentMeetingOption MeetingRecurrenceOption { get; }

        /// <summary>
        /// Gets or sets the duration of the recurrent meeting.
        /// </summary>
        /// <value>The duration of the recurrent meeting.</value>
        TimeSpan MeetingDuration();

        /// <summary>
        /// Adds the recurrent option.
        /// </summary>
        void SetRecurrentOption(IRecurrentMeetingOption recurrentMeetingOption);

        /// <summary>
        /// Removes the recurrent option.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/9/2008
        /// </remarks>
        void RemoveRecurrentOption();

        /// <summary>
        /// Gets the recurring dates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/9/2008
        /// </remarks>
        IList<DateOnly> GetRecurringDates();

        /// <summary>
        /// Gets the meeting period for the requested date.
        /// </summary>
        /// <param name="meetingDay">The meeting day.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        DateTimePeriod MeetingPeriod(DateOnly meetingDay);

        /// <summary>
        /// Sets the scenario.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        void SetScenario(IScenario scenario);

        /// <summary>
        /// Gets or sets the original meeting id.
        /// Is used when exporting Meetings to another Scenario.
        /// </summary>
        /// <value>
        /// The original meeting id.
        /// </value>
        Guid OriginalMeetingId { get; set; }

		/// <summary>
		/// Make sure that a snapshot is taken for this meeting.
		/// </summary>
    	void Snapshot();
    }
}
