using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    class LogOnCommand : ICommand
    {
        private readonly SimpleSampleViewModel _simpleSampleViewModel;

        public LogOnCommand(SimpleSampleViewModel simpleSampleViewModel)
        {
            _simpleSampleViewModel = simpleSampleViewModel;
            _simpleSampleViewModel.PropertyChanged += _simpleSampleViewModel_PropertyChanged;
        }

        private void _simpleSampleViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = CanExecuteChanged;
            if (handler!=null)
            {
                handler.Invoke(this,EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {

            var logOnService = new ChannelFactory<ITeleoptiCccLogOnService>(typeof(ITeleoptiCccLogOnService).Name).CreateChannel();
            var dataSources = logOnService.GetDataSources();
            var firstDataSource = dataSources.First();
            var result = logOnService.LogOnApplicationUser(_simpleSampleViewModel.UserName, _simpleSampleViewModel.Password,
                                              firstDataSource);

            if (result.Successful)
            {
                AuthenticationMessageHeader.BusinessUnit = result.BusinessUnitCollection.First().Id.GetValueOrDefault();
                AuthenticationMessageHeader.DataSource = firstDataSource.Name;
                AuthenticationMessageHeader.Password = _simpleSampleViewModel.Password;
                AuthenticationMessageHeader.UserName = _simpleSampleViewModel.UserName;
                AuthenticationMessageHeader.UseWindowsIdentity = false;
            }

            _simpleSampleViewModel.EnableFunctions = result.Successful;
        }

        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrWhiteSpace(_simpleSampleViewModel.UserName) &&
                   !string.IsNullOrWhiteSpace(_simpleSampleViewModel.Password);
        }

        public event EventHandler CanExecuteChanged;
    }
}
