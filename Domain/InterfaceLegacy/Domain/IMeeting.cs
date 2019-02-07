using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Meeting
    /// </summary>
    public interface IMeeting : IAggregateRoot, ICloneableEntity<IMeeting>, IChangeInfo, IFilterOnBusinessUnit
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
        /// Description
        /// </summary>
        string Description { set; }

    	/// <summary>
    	/// Get the formatted description
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetDescription(ITextFormatter formatter);

        /// <summary>
        /// Location
        /// </summary>
        string Location { set; }

    	/// <summary>
    	/// Get the formatted location
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetLocation(ITextFormatter formatter);

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
		/// <param name="period"></param>
		/// <param name="person"></param>
		/// <returns></returns>
		IList<IPersonMeeting> GetPersonMeetings(DateTimePeriod period, params IPerson[] person);

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
        string Subject { set; }

    	/// <summary>
    	/// Get the formatted subject
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetSubject(ITextFormatter formatter);

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
        TimeZoneInfo TimeZone { get; set; }

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
