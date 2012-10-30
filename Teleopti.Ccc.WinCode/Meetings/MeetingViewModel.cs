using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
	public class MeetingViewModel : INotifyPropertyChanged, IMeetingViewModel
	{
        private readonly IMeeting _meeting;
        private readonly IMeeting _originalMeeting;
        private readonly CommonNameDescriptionSetting _commonNameSetting;
        private IList<ContactPersonViewModel> _requiredParticipants;
        private IList<ContactPersonViewModel> _optionalParticipants;
        private RecurrentMeetingOptionViewModel _recurringOption;

		public MeetingViewModel(IMeeting meeting, CommonNameDescriptionSetting commonNameSetting)
        {
            _originalMeeting = meeting;
            _meeting = _originalMeeting.EntityClone();
			_meeting.Snapshot();

            _commonNameSetting = commonNameSetting;
            _recurringOption = RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(this,
                                                                                                            _meeting.
                                                                                                                MeetingRecurrenceOption);
            RecreateParticipantLists();
        }

        public IMeeting OriginalMeeting
        {
            get { return _originalMeeting; }
        }

        private void RecreateParticipantLists()
        {
            _requiredParticipants =
                new List<ContactPersonViewModel>(
                    ContactPersonViewModel.Parse(_meeting.MeetingPersons.Where(m => !m.Optional).Select(p => p.Person), _commonNameSetting));
            _optionalParticipants =
                new List<ContactPersonViewModel>(
                    ContactPersonViewModel.Parse(_meeting.MeetingPersons.Where(m => m.Optional).Select(p => p.Person), _commonNameSetting));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public IMeeting Meeting
        {
            get { return _meeting; }
        }

        public string Organizer
        {
            get { return _commonNameSetting.BuildCommonNameDescription(_meeting.Organizer); }
        }

        public DateOnly StartDate
        {
            get { return _meeting.StartDate; }
            set
            {
                if (_meeting.StartDate != value)
                {
                    _meeting.StartDate = value;
                    NotifyPropertyChanged("StartDate");
                }
            }
        }

        public DateOnly EndDate
        {
            get
            {
                return new DateOnly(_meeting.StartDate.Date.Add(_meeting.EndTime));
            }
        }

        public DateOnly RecurringEndDate
        {
            get { return _meeting.EndDate; }
            set
            {
                if (value < _meeting.StartDate)
                    value = _meeting.StartDate;
                if (_meeting.EndDate != value)
                {
                    _meeting.EndDate = value;
                    NotifyPropertyChanged("RecurringEndDate");
                }
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                if (_meeting.EndTime >= TimeSpan.FromDays(1))
                    return _meeting.EndTime.Add(TimeSpan.FromDays(-1));

                return _meeting.EndTime;
            }
            set
            {
                if (value != EndTime)
                {
                    if (value <= StartTime)
                        value = value.Add(TimeSpan.FromDays(1));

                    _meeting.EndTime = value;
                    NotifyPropertyChanged("EndTime");
                }
            }
        }

        public TimeSpan StartTime
        {
            get { return _meeting.StartTime; }
            set
            {
                if (_meeting.StartTime != value)
                {
                    _meeting.StartTime = value;
                    NotifyPropertyChanged("StartTime");
                }
            }
        }

        public TimeSpan MeetingDuration
        {
            get { return _meeting.MeetingDuration(); }
            set
            {
                if (value != _meeting.MeetingDuration())
                {
                    _meeting.EndTime = _meeting.StartTime.Add(value);
                    NotifyPropertyChanged("MeetingDuration");
                }
            }
        }

        public string Location
        {
            set
            {
                if (_meeting.GetLocation(new NoFormatting()) != value)
                {
                    _meeting.Location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }

		public string GetLocation(ITextFormatter formatter)
		{
			return _meeting.GetLocation(formatter);
		}

        public string Subject
        {
            set
            {
                if (_meeting.GetSubject(new NoFormatting()) != value)
                {
                    _meeting.Subject = value;
                    NotifyPropertyChanged("Subject");
                }
            }
        }

		public string GetSubject(ITextFormatter formatter)
		{
			return _meeting.GetSubject(formatter);
		}

        public string Description
        {
            set
            {
                if (_meeting.GetDescription(new NoFormatting()) != value)
                {
                    _meeting.Description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

		public string GetDescription(ITextFormatter formatter)
		{
			return _meeting.GetDescription(formatter);
		}

        public IActivity Activity
        {
            get { return _meeting.Activity; }
            set
            {
                if (_meeting.Activity != value)
                {
                    _meeting.Activity = value;
                    NotifyPropertyChanged("Activity");
                }
            }
        }

        public TimeZoneInfo TimeZone
        {
            get { return _meeting.TimeZone; }
            set
            {
                if (_meeting.TimeZone.Id != value.Id)
                {
                    _meeting.TimeZone = value;
                    NotifyPropertyChanged("TimeZone");
                }
            }
        }

        public string Participants
        {
            get {
                return String.Join(ParticipantSeparator, _requiredParticipants.Concat(_optionalParticipants).Select(p => p.FullName).ToArray()); }
        }

        public IList<ContactPersonViewModel> RequiredParticipants
        {
            get { return new List<ContactPersonViewModel>(_requiredParticipants); }
        }

        public IList<ContactPersonViewModel> OptionalParticipants
        {
            get { return new List<ContactPersonViewModel>(_optionalParticipants); }
        }

        public RecurrentMeetingOptionViewModel RecurringOption
        {
            get {
                return _recurringOption;
            }
            set {
                _recurringOption = value;
                _meeting.SetRecurrentOption(_recurringOption.RecurrentMeetingOption);
                NotifyPropertyChanged("RecurringOption");
            }
        }

        public bool IsRecurring
        {
            get { return _meeting.EndDate > _meeting.StartDate; }
        }

        public const string ParticipantSeparator = "; ";

        protected void NotifyPropertyChanged(string property)
        {
			var handler = PropertyChanged;
            if (handler != null)
            {
            	handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public void AddParticipants(IList<ContactPersonViewModel> requiredViewModels, IList<ContactPersonViewModel> optionalViewModels)
        {
            _meeting.ClearMeetingPersons();
            foreach (ContactPersonViewModel contactPersonViewModel in requiredViewModels)
            {
                var meetingPerson =
                    _meeting.MeetingPersons.FirstOrDefault(p => p.Person.Equals(contactPersonViewModel.ContainedEntity));
                if (meetingPerson==null)
                {
                    _meeting.AddMeetingPerson(new MeetingPerson(contactPersonViewModel.ContainedEntity,false));
                }
            }

            foreach (ContactPersonViewModel contactPersonViewModel in optionalViewModels)
            {
                var meetingPerson =
                    _meeting.MeetingPersons.FirstOrDefault(p => p.Person.Equals(contactPersonViewModel.ContainedEntity));
                if (meetingPerson == null)
                {
                    _meeting.AddMeetingPerson(new MeetingPerson(contactPersonViewModel.ContainedEntity, true));
                }
            }
            RecreateParticipantLists();
            NotifyPropertyChanged("Participants");
        }

        public void RemoveParticipant(EntityContainer<IPerson> contactPersonViewModel)
        {
            _meeting.RemovePerson(contactPersonViewModel.ContainedEntity);
            RecreateParticipantLists();
            NotifyPropertyChanged("Participants");
        }

        public void RemoveRecurrence()
        {
            _meeting.RemoveRecurrentOption();
            _recurringOption = RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(this,
                                                                                                            _meeting.
                                                                                                                MeetingRecurrenceOption);
            NotifyPropertyChanged("RecurringOption");
        }

		public TimeSpan SlotStartTime { get; set; }

		public TimeSpan SlotEndTime { get; set; }
	}
}
