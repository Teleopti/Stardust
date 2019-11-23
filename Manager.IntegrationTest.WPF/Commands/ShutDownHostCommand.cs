using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Manager.IntegrationTest.WPF.ViewModels;

namespace Manager.IntegrationTest.WPF.Commands
{
	public class ShutDownHostCommand : ICommand
	{
		public ShutDownHostCommand(MainWindowViewModel mainWindowViewModel)
		{
			if (mainWindowViewModel == null)
			{
				throw new ArgumentNullException(nameof(mainWindowViewModel));
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
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
    }
}