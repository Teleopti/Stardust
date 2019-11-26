using System;
using System.ComponentModel;
using System.Windows.Input;
using Manager.Integration.Test.WPF.ViewModels;

namespace Manager.Integration.Test.WPF.Commands
{
	public class ShutDownHostCommand : ICommand
	{
		public ShutDownHostCommand(MainWindowViewModel mainWindowViewModel)
		{
			if (mainWindowViewModel == null)
			{
				throw new ArgumentNullException("mainWindowViewModel");
			}

			MainWindowViewModel = mainWindowViewModel;

			MainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;

		}

		private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("IsConsoleHostStarted", StringComparison.InvariantCultureIgnoreCase))
			{
				OnCanExecuteChanged();
			}
		}

		public MainWindowViewModel MainWindowViewModel { get; set; }

		public bool CanExecute(object parameter)
		{
			return MainWindowViewModel.IsConsoleHostStarted;
		}

		public void Execute(object parameter)
		{
			MainWindowViewModel.ShutDownConsoleHost();
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