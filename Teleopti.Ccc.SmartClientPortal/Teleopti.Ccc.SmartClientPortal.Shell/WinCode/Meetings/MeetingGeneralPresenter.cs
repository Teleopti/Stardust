using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
	public class MeetingGeneralPresenter : IMeetingDetailPresenter
    {
        private readonly IMeetingGeneralView _view;
        private readonly IMeetingViewModel _model;

        private static readonly IList<TimeZoneInfo> TimeZoneList =
            TimeZoneInfo.GetSystemTimeZones().Select(t => t).ToList();

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
			SetActivity(_model.Activity);
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
			if (updateFlag) return;
            _model.EndTime = endTime;
            _view.SetEndTime(_model.EndTime);
        }

        public void SetStartTime(TimeSpan startTime)
        {
			if (updateFlag) return;
            _model.StartTime = startTime;
			_view.SetEndTime(_model.EndTime);
        }

        public void SetActivity(IActivity activity)
        {
            _model.Activity = activity;
        }

        public void SetDescription(string description)
        {
            _model.Description = description;
        }

        public void OnParticipantsSet()
        {
            _view.SetParticipants(_model.Participants);
        }


		private bool updateFlag;
		public void UpdateView()
		{
			updateFlag = true;
			_view.DescriptionFocus();
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
			_view.SetSubject(_model.GetSubject(new NoFormatting()));
			_view.SetLocation(_model.GetLocation(new NoFormatting()));
			_view.SetDescription(_model.GetDescription(new NoFormatting()));
			updateFlag = false;
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
                    if (timeSpan.Value >= TimeSpan.Zero)
                    {
                        SetStartTime(timeSpan.Value);
                        _view.NotifyMeetingTimeChanged();
                    }
                    else
                        _view.SetStartTime(_model.StartTime);
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
                    if (timeSpan.Value >= TimeSpan.Zero)
                    {
                        SetEndTime(timeSpan.Value);
                        _view.NotifyMeetingTimeChanged();
                    }
                    else
                        _view.SetEndTime(_model.EndTime);

				}
			}
			else
				_view.SetEndTime(_model.EndTime);

			_settingTime = false;
		}

		public void Remove(IList<int> allSelectedIndexes)
		{
			var optionalOffset = Model.RequiredParticipants.Count;
			IList<ContactPersonViewModel> selected = new List<ContactPersonViewModel>();
			foreach (var index in allSelectedIndexes)
			{
				if (index < Model.RequiredParticipants.Count)
					selected.Add(Model.RequiredParticipants[index]);
				else if ((index - optionalOffset) < Model.OptionalParticipants.Count)
					selected.Add(Model.OptionalParticipants[index - optionalOffset]);
			}

			foreach (var contactPersonViewModel in selected)
			{
				Model.RemoveParticipant(contactPersonViewModel);
			}

			_view.SetParticipants(Model.Participants);
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
