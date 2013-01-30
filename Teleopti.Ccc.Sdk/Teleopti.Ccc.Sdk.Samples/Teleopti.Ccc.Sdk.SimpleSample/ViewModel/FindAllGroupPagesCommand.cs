using System;
using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
	public class FindAllGroupPagesCommand : ICommand
	{
		private readonly ScheduleViewModel _scheduleViewModel;
		private readonly GroupPageRepository _groupPageRepository;

		public FindAllGroupPagesCommand(ScheduleViewModel scheduleViewModel, GroupPageRepository groupPageRepository)
		{
			_scheduleViewModel = scheduleViewModel;
			_groupPageRepository = groupPageRepository;
		}

		public void Execute(object parameter)
		{
			_scheduleViewModel.GroupPages.Clear();

			foreach (var groupPage in _groupPageRepository.GetGroupPages())
			{
				_scheduleViewModel.GroupPages.Add(groupPage);
			}

			_scheduleViewModel.SelectedGroupPage = _scheduleViewModel.GroupPages.FirstOrDefault();
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;
	}
}