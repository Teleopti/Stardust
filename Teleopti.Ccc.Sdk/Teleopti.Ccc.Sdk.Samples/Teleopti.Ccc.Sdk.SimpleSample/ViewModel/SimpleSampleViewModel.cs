using System.ComponentModel;
using System.Windows.Input;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    public class SimpleSampleViewModel : INotifyPropertyChanged
    {
        private bool _enableFunctions;
        private string _userName;
        private string _password;

        public SimpleSampleViewModel()
        {
            PeopleViewModel = new PeopleViewModel();
            ScheduleViewModel = new ScheduleViewModel();
            LogOnCommand = new LogOnCommand(this);
        }

        public bool EnableFunctions
        {
            get { return _enableFunctions; }
            set
            {
                _enableFunctions = value;
                notifyPropertyChanged("EnableFunctions");
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                notifyPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                notifyPropertyChanged("Password");
            }
        }

        public ICommand LogOnCommand { get; private set; }

        public PeopleViewModel PeopleViewModel { get; private set; }

        public ScheduleViewModel ScheduleViewModel { get; private set; }

        protected virtual void notifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
