using System;
using System.ComponentModel;
using System.Windows.Input;
using Manager.Integration.Test.WPF.ViewModels;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartHostCommand : ICommand
	{
		public StartHostCommand(MainWindowViewModel mainWindowViewModel)
		{
			if (mainWindowViewModel == null)
			{
				throw new ArgumentNullException("mainWindowViewModel");
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
			{
				handler(this, System.EventArgs.Empty);
			}
		}
	}
}