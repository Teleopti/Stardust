using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;



namespace Teleopti.Ccc.WpfControls.Common.Models
{
    public class DataModel : INotifyPropertyChanged
    {
        #region fields
        private Dispatcher _dispatcher;
        private ModelState _state;
        private PropertyChangedEventHandler _propertyChangedEvent;
        #endregion //fields

        #region properties & events
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
                    SendPropertyChanged("State");
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
        #endregion //properties & events

      
        public DataModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region methods
        protected void SendPropertyChanged(string property)
        {
          if (_propertyChangedEvent!=null)_propertyChangedEvent(this,new PropertyChangedEventArgs(property));
           
        }
       

        [Conditional("Debug")]
        protected void VerifyCalledOnUIThread()
        {
            Debug.Assert(Dispatcher.CurrentDispatcher == this.Dispatcher, "Call must be made on current Dispatcher");

        }
        #endregion //methods

    }
}
