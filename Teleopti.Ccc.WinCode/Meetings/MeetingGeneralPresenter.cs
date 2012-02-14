using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings
{
	public class MeetingGeneralPresenter : IMeetingDetailPresenter
    {
        private readonly IMeetingGeneralView _view;
        private readonly IMeetingViewModel _model;

        private static readonly IList<ICccTimeZoneInfo> TimeZoneList =
            TimeZoneInfo.GetSystemTimeZones().Select(t => (ICccTimeZoneInfo) new CccTimeZoneInfo(t)).ToList();

	    private bool _disposed;

	    public MeetingGeneralPresenter(IMeetingGeneralView view, IMeetingViewModel model)
        {
            _view = view;
            _model = model;
            RepositoryFactory = new RepositoryFactory();
            UnitOfWorkFactory = Infrastructure.UnitOfWork.UnitOfWorkFactory.Current;
        }

        public IMeetingViewModel Model
        {
            get { return _model; }
        }

        protected IRepositoryFactory RepositoryFactory { get; set;}
        protected IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public void Initialize()
        {
            IList<IActivity> activities;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                activities = RepositoryFactory.CreateActivityRepository(unitOfWork).LoadAllSortByName();
            }
			_view.SetActivityList(activities);
			UpdateView();
            
        }

        public void SetSubject(string subject)
        {
            _model.Subject = subject;
        }

        public void SetLocation(string location)
        {
            _model.Location = location;
        }

        public void SetStartDate(DateOnly startDate)
        {
            bool isRecurring = _model.IsRecurring;
            _model.StartDate = startDate;
            _view.SetEndDate(_model.EndDate);
            if (!isRecurring)
            {
                _model.RecurringEndDate = _model.StartDate;
                _view.SetRecurringEndDate(_model.RecurringEndDate);
            }
        }

        public void SetEndTime(TimeSpan endTime)
        {
            _model.EndTime = endTime;
            _view.SetEndDate(_model.EndDate);
        }

        public void SetStartTime(TimeSpan startTime)
        {
            _model.StartTime = startTime;
            _view.SetEndDate(_model.EndDate);
        }

        public void SetActivity(IActivity activity)
        {
            _model.Activity = activity;
        }

        public void SetDescription(string description)
        {
            _model.Description = description;
        }

        public void ParseParticipants(string participantText)
        {
            var allParticipants = _model.RequiredParticipants.Concat(_model.OptionalParticipants).ToList();
            var participantsFromText = participantText.Split(new[] { MeetingViewModel.ParticipantSeparator },
                                                             StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < participantsFromText.Length; i++)
            {
                participantsFromText[i] = participantsFromText[i].Trim();
            }
            foreach (var viewModel in allParticipants)
            {
                var foundName = false;
                if (participantsFromText.Contains(viewModel.FullName.Trim())) continue;
                for (var j = 0; j < participantsFromText.Length; j++)
                {
                    if (participantsFromText[j].Length <= viewModel.FullName.Trim().Length) continue;
                    var itemEnding = participantsFromText[j].Substring(
                         0, viewModel.FullName.Trim().Length);
                    if (viewModel.FullName.Trim() == itemEnding)
                    {
                        foundName = true;
                    }
                }
                if (!foundName)
                {
                    _model.RemoveParticipant(viewModel);
                }
            }
            _view.SetParticipants(_model.Participants);
        }

        public void OnParticipantsSet()
        {
            _view.SetParticipants(_model.Participants);
        }

		public void UpdateView()
		{
			_view.SetOrganizer(_model.Organizer);
			_view.SetParticipants(_model.Participants);
			
			_view.SetSelectedActivity(_model.Activity);
			_view.SetTimeZoneList(TimeZoneList);
			_view.SetSelectedTimeZone(_model.TimeZone);
			_view.SetStartDate(_model.StartDate);
			_view.SetEndDate(_model.EndDate);
			_view.SetStartTime(_model.StartTime);
			_view.SetEndTime(_model.EndTime);
			_view.SetRecurringEndDate(_model.RecurringEndDate);
			_view.SetSubject(_model.Subject);
			_view.SetLocation(_model.Location);
			_view.SetDescription(_model.Description);
		}

	    public void CancelAllLoads()
	    {
	        //cancel backgroud worker
	    }

		// to avoid recursive
		private bool _settingTime;
	    public void OnOutlookTimePickerStartTimeLeave(string inputText)
		{
			if (_settingTime) return;
	    	_settingTime = true;
			TimeSpan? timeSpan;

			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
				if (timeSpan.HasValue)
				{
					SetStartTime(timeSpan.Value);
					_view.NotifyMeetingTimeChanged();
				}
			}
			else
				_view.SetStartTime(_model.StartTime);

	    	_settingTime = false;
		}

		public void OnOutlookTimePickerStartTimeKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerStartTimeLeave(inputText);
		}

		public void OnOutlookTimePickerEndTimeLeave(string inputText)
		{
			if (_settingTime) return;
			_settingTime = true;
			TimeSpan? timeSpan;

			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
				if (timeSpan.HasValue)
				{
					SetEndTime(timeSpan.Value);
					_view.NotifyMeetingTimeChanged();
				}
			}
			else
				_view.SetEndTime(_model.EndTime);

			_settingTime = false;
		}

		public void OnOutlookTimePickerEndTimeKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerEndTimeLeave(inputText);
		}

        #region IDispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ReleaseManagedResources();
                }
                ReleaseUnmanagedResources();
                _disposed = true;
            }
        }

        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void ReleaseManagedResources()
        {}

        #endregion
    }
}
