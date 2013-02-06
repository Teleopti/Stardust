using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
	public class AfterSuccessfulLogOnCommand : ICommand
	{
		private readonly SimpleSampleViewModel _simpleSampleViewModel;

		public AfterSuccessfulLogOnCommand(SimpleSampleViewModel simpleSampleViewModel)
		{
			_simpleSampleViewModel = simpleSampleViewModel;
			_simpleSampleViewModel.PropertyChanged +=_simpleSampleViewModel_PropertyChanged;
		}

		private void _simpleSampleViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "EnableFunctions")
			{
				var handler = CanExecuteChanged;
				if (handler!=null)
					handler.Invoke(this,EventArgs.Empty);
			}
		}

		public void Execute(object parameter)
		{
			_simpleSampleViewModel.ScheduleViewModel.LoadGroupPages.Execute(null);
		}

		public bool CanExecute(object parameter)
		{
			return _simpleSampleViewModel.EnableFunctions && _simpleSampleViewModel.ScheduleViewModel.LoadGroupPages.CanExecute(null);
		}

		public event EventHandler CanExecuteChanged;
	}
}