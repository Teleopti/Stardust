using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public interface IMeetingChangerAndPersister
    {
        void ChangeDurationAndPersist(IMeeting meeting, TimeSpan durationChange, IViewBase view);

        void ChangeStartDateTimeAndPersist(IMeeting meeting, DateTime userStartDateTime, TimeSpan durationChange,
                                           TimeZoneInfo userTimeZone, IViewBase view);
    }

    public class MeetingChangerAndPersister : IMeetingChangerAndPersister
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMeetingOverviewViewModel _model;
        private readonly IMeetingParticipantPermittedChecker _meetingParticipantPermittedChecker;

        public MeetingChangerAndPersister(IMeetingRepository meetingRepository, IUnitOfWorkFactory unitOfWorkFactory, 
            IMeetingOverviewViewModel model, IMeetingParticipantPermittedChecker meetingParticipantPermittedChecker)
        {
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _model = model;
            _meetingParticipantPermittedChecker = meetingParticipantPermittedChecker;
        }

        public void ChangeDurationAndPersist(IMeeting meeting, TimeSpan durationChange, IViewBase view)
        {
            if(meeting != null)
            {
                var newDuration = meeting.MeetingDuration().Add(durationChange);
                meeting.EndTime = meeting.StartTime.Add(newDuration);
                persistMeeting(meeting, view);
            }
            
        }

        public void ChangeStartDateTimeAndPersist(IMeeting meeting, DateTime userStartDateTime, TimeSpan durationChange, TimeZoneInfo userTimeZone, IViewBase view)
        {
            if (meeting != null)
            {
                var meetingTimeZoneHelper = new MeetingTimeZoneHelper(userTimeZone);
                var meetingTimeZone = meeting.TimeZone;
                var meetingStartDateTime = meetingTimeZoneHelper.ConvertToMeetingTimeZone(userStartDateTime,
                                                                                          meetingTimeZone);
                var newDuration = meeting.MeetingDuration().Add(durationChange);
                if (newDuration <= TimeSpan.FromMinutes(0))
                    newDuration = TimeSpan.FromMinutes(15);
                //inte recurring
                if (meeting.GetRecurringDates().Count == 1)
                {
                    meeting.StartDate = new DateOnly(meetingStartDateTime);
                    meeting.EndDate = new DateOnly(meetingStartDateTime);
                }

                meeting.StartTime = meetingStartDateTime.TimeOfDay;
                meeting.EndTime = meetingStartDateTime.TimeOfDay.Add(newDuration);
				if (meeting.EndTime.Days > meeting.StartTime.Days)
					meeting.EndTime = TimeSpan.Zero;

                // change scenario if we paste it to another
                meeting.SetScenario(_model.CurrentScenario);

                persistMeeting(meeting, view);
            }
        }

        private void persistMeeting(IMeeting meeting, IViewBase view)
        {
            var meetingId = meeting.Id;

            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var persons = meeting.MeetingPersons.Select(m => m.Person);
                unitOfWork.Reassociate(persons);
                if (!_meetingParticipantPermittedChecker.ValidatePermittedPersons(persons, _model.SelectedPeriod.StartDate, view, PrincipalAuthorization.Current()))
                    return;
                unitOfWork.Reassociate(meeting.BusinessUnit);
                if (!meetingId.HasValue)
                {
                    _meetingRepository.Add(meeting);
                }
                else
                {
                	var mergeMeeting = _meetingRepository.Load(meetingId.Value);
					mergeMeeting.Snapshot();
                    unitOfWork.Merge(meeting);
                }
                unitOfWork.PersistAll();
            }
        }

        
    }
}