using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models
{
    public class DataModel : INotifyPropertyChanged
    {
        private Dispatcher _dispatcher;
        private ModelState _state;
        private PropertyChangedEventHandler _propertyChangedEvent;
     
        public Dispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        public ModelState State
        {
            get
            {
                VerifyCalledOnUIThread();
                return _state;
            }
            set
            {
                VerifyCalledOnUIThread();
                if (value != _state)
                {
                    _state = value;
                    SendPropertyChanged(nameof(State));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                VerifyCalledOnUIThread();
                _propertyChangedEvent += value;
            }
            remove
            {
                VerifyCalledOnUIThread();
                _propertyChangedEvent -= value;
            }
        }
        
        public DataModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected void SendPropertyChanged(string property)
        {
            _propertyChangedEvent?.Invoke(this, new PropertyChangedEventArgs(property));
        }
		
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Conditional("Debug")]
        protected void VerifyCalledOnUIThread()
        {
            Debug.Assert(Dispatcher.CurrentDispatcher == this.Dispatcher, "Call must be made on current Dispatcher");
        }
    }
}
