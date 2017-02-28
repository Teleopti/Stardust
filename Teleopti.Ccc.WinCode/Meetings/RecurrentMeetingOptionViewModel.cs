using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public abstract class RecurrentMeetingOptionViewModel : INotifyPropertyChanged
    {
        private readonly MeetingViewModel _meetingViewModel;
        private readonly IRecurrentMeetingOption _recurrentMeetingOption;

        protected RecurrentMeetingOptionViewModel(MeetingViewModel meetingViewModel, IRecurrentMeetingOption recurrentMeetingOption)
        {
            _meetingViewModel = meetingViewModel;
            _recurrentMeetingOption = recurrentMeetingOption;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int IncrementCount
        {
            get { return _recurrentMeetingOption.IncrementCount; }
            set
            {
                _recurrentMeetingOption.IncrementCount = value;
                NotifyPropertyChanged(nameof(IncrementCount));
            }
        }

        public IRecurrentMeetingOption RecurrentMeetingOption
        {
            get { return _recurrentMeetingOption; }
        }

        protected MeetingViewModel MeetingViewModel
        {
            get { return _meetingViewModel; }
        }

        public abstract RecurrentMeetingType RecurrentMeetingType { get;}

        public virtual RecurrentMeetingOptionViewModel ChangeRecurringMeetingOption(RecurrentMeetingType recurrentMeetingType)
        {
            RecurrentMeetingOptionViewModel viewModel = null;
            switch (recurrentMeetingType)
            {
                case RecurrentMeetingType.Daily:
                    viewModel = 
                        new RecurrentDailyMeetingViewModel(MeetingViewModel,
                                                           new RecurrentDailyMeeting {IncrementCount = IncrementCount});
                    break;
                case RecurrentMeetingType.Weekly:
                    RecurrentWeeklyMeetingViewModel recurrentWeeklyMeetingViewModel =
                        new RecurrentWeeklyMeetingViewModel(MeetingViewModel,
                                                            new RecurrentWeeklyMeeting {IncrementCount = IncrementCount});
                    recurrentWeeklyMeetingViewModel[MeetingViewModel.StartDate.DayOfWeek] = true;
                    viewModel = recurrentWeeklyMeetingViewModel;
                    break;
                case RecurrentMeetingType.MonthlyByDay:
                    RecurrentMonthlyByDayMeetingViewModel recurrentMonthlyByDayMeetingViewModel =
                        new RecurrentMonthlyByDayMeetingViewModel(MeetingViewModel,
                                                                  new RecurrentMonthlyByDayMeeting {IncrementCount = IncrementCount});
                    recurrentMonthlyByDayMeetingViewModel.DayInMonth =
                        System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(
                            MeetingViewModel.StartDate.Date);
                    viewModel = recurrentMonthlyByDayMeetingViewModel;
                    break;
                case RecurrentMeetingType.MonthlyByWeek:
                    RecurrentMonthlyByWeekMeetingViewModel recurrentMonthlyByWeekMeetingViewModel =
                        new RecurrentMonthlyByWeekMeetingViewModel(MeetingViewModel,
                                                                  new RecurrentMonthlyByWeekMeeting { IncrementCount = IncrementCount });
                    recurrentMonthlyByWeekMeetingViewModel.DayOfWeek = MeetingViewModel.StartDate.DayOfWeek;
                    viewModel = recurrentMonthlyByWeekMeetingViewModel;
                    break;
            }
            return viewModel;
        }

        protected void NotifyPropertyChanged(string property)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}
        }

        public abstract bool IsValid();
    }
}