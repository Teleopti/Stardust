using System;
using System.Windows.Input;
using Manager.Integration.Test.WPF.ViewModels;

namespace Manager.Integration.Test.WPF.Commands
{
    public class ClearDatabaseCommand : ICommand
    {
        public MainWindowViewModel MainWindowViewModel { get; private set; }

        public ClearDatabaseCommand(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;

            //MainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object sender,
                                                         System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
            return MainWindowViewModel.DatabaseContainsInformation();
        }

        public void Execute(object parameter)
        {
            MainWindowViewModel.DatabaseClearAllInformation();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this,
                                  EventArgs.Empty);
            }
        }
    }
}