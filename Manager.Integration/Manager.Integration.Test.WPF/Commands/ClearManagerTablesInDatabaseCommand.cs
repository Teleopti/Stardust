using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Manager.Integration.Test.WPF.ViewModels;

namespace Manager.Integration.Test.WPF.Commands
{
    public class ClearManagerTablesInDatabaseCommand : ICommand
    {
        public MainWindowViewModel MainWindowViewModel { get; private set; }

        public ClearManagerTablesInDatabaseCommand(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;

            //MainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object sender,
                                                         PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
	        return true;
        }

        public void Execute(object parameter)
        {
            MainWindowViewModel.ClearAllManagerTablesInDatabase();
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