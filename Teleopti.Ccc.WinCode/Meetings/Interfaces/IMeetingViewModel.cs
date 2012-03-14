using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
	public interface IMeetingViewModel
	{
		IMeeting OriginalMeeting { get; }
		IMeeting Meeting { get; }
		string Organizer { get; }
		DateOnly StartDate { get; set; }
		DateOnly EndDate { get; }
		DateOnly RecurringEndDate { get; set; }
		TimeSpan EndTime { get; set; }
		TimeSpan StartTime { get; set; }
		TimeSpan MeetingDuration { get; set; }
		string Location { get; set; }
		string Subject { get; set; }
		string Description { get; set; }
		IActivity Activity { get; set; }
		ICccTimeZoneInfo TimeZone { get; set; }
		string Participants { get; }
		ReadOnlyCollection<ContactPersonViewModel> RequiredParticipants { get; }
		ReadOnlyCollection<ContactPersonViewModel> OptionalParticipants { get; }
		RecurrentMeetingOptionViewModel RecurringOption { get; set; }
		bool IsRecurring { get; }
		event PropertyChangedEventHandler PropertyChanged;
		void AddParticipants(IList<ContactPersonViewModel> requiredViewModels, IList<ContactPersonViewModel> optionalViewModels);
		void RemoveParticipant(EntityContainer<IPerson> contactPersonViewModel);
		void RemoveRecurrence();
		TimeSpan SlotStartTime { get; set; }
		TimeSpan SlotEndTime { get; set; }
	}
}