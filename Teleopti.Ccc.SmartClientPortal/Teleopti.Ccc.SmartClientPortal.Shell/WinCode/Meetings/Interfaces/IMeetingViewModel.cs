using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
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
		string Location { set; }
		string GetLocation(ITextFormatter formatter);
		string Subject { set; }
		string GetSubject(ITextFormatter formatter);
		string Description { set; }
		string GetDescription(ITextFormatter formatter);
		IActivity Activity { get; set; }
		TimeZoneInfo TimeZone { get; set; }
		string Participants { get; }
        IList<ContactPersonViewModel> RequiredParticipants { get; }
        IList<ContactPersonViewModel> OptionalParticipants { get; }
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