using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Manager.IntegrationTest.WPF.ViewModels;

namespace Manager.IntegrationTest.WPF.Commands
{
	public class StartHostCommand : ICommand
	{
		public StartHostCommand(MainWindowViewModel mainWindowViewModel)
		{
			if (mainWindowViewModel == null)
			{
				throw new ArgumentNullException(nameof(mainWindowViewModel));
			}

			MainWindowViewModel = mainWindowViewModel;

			MainWindowViewModel.PropertyChanged +=MainWindowViewModelOnPropertyChanged;
		}

		private void MainWindowViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName.Equals("IsConsoleHostStarted", StringComparison.InvariantCultureIgnoreCase))
			{
				OnCanExecuteChanged();
			}
		}

		public MainWindowViewModel MainWindowViewModel { get; set; }

		public bool CanExecute(object parameter)
		{
			return !MainWindowViewModel.IsConsoleHostStarted;
		}

		public void Execute(object parameter)
		{
			MainWindowViewModel.StartConsoleHost();
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