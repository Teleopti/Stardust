using System;
using System.Windows;
using System.Windows.Input;
using Manager.Integration.Test.WPF.ViewModels;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartDurationTestCommand : ICommand
	{
		public MainWindowViewModel MainWindowViewModel { get; private set; }

		public StartDurationTestCommand(MainWindowViewModel mainWindowViewModel)
		{
			MainWindowViewModel = mainWindowViewModel;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			MainWindowViewModel.StartDurationTest();
		}

		public event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
	}
}