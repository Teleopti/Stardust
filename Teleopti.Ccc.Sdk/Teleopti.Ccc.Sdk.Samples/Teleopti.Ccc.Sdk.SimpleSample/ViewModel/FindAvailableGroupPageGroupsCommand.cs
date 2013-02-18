using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
	public class FindAvailableGroupPageGroupsCommand : ICommand
	{
		private readonly ScheduleViewModel _scheduleViewModel;
		private readonly GroupPageRepository _groupPageRepository;

		public FindAvailableGroupPageGroupsCommand(ScheduleViewModel scheduleViewModel, GroupPageRepository groupPageRepository)
		{
			_scheduleViewModel = scheduleViewModel;
			_groupPageRepository = groupPageRepository;
			_scheduleViewModel.PropertyChanged += ScheduleViewModelOnPropertyChanged;
		}

		private void ScheduleViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "SelectedGroupPage" ||
				propertyChangedEventArgs.PropertyName == "StartDate")
			{
				var handler = CanExecuteChanged;
				if (handler!=null)
				{
					handler(this, EventArgs.Empty);
				}

				_scheduleViewModel.FoundSchedules.Clear();
				if (CanExecute(null))
				{
					Execute(null);
				}
			}
		}

		public void Execute(object parameter)
		{
			_scheduleViewModel.GroupPageGroups.Clear();

			foreach (var groupPageGroup in _groupPageRepository.GetGroupPageGroups(_scheduleViewModel.StartDate,_scheduleViewModel.SelectedGroupPage))
			{
				_scheduleViewModel.GroupPageGroups.Add(groupPageGroup);
			}

			_scheduleViewModel.SelectedGroupPageGroup = _scheduleViewModel.GroupPageGroups.FirstOrDefault();
		}

		public bool CanExecute(object parameter)
		{
			return _scheduleViewModel.SelectedGroupPage != null;
		}

		public event EventHandler CanExecuteChanged;
	}
}