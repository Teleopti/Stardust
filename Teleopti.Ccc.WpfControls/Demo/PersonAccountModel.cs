using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Teleopti.Ccc.WpfControls.Demo.Models;

namespace Teleopti.Ccc.WpfControls.Demo
{
    public class PersonAccountModel : DataModel
    {
        private IFetchableModel<int> _tracker;
        private int _balanceIn;
        private int _used;

        public int BalanceIn
        {
            get { return _balanceIn; }
            set
            {
                VerifyCalledOnUIThread();
                if (_balanceIn != value)
                {
                    _balanceIn = value;
                    SendPropertyChanged("BalanceIn");
                }
            }
        }
        public int Used
        {
            get { return _used; }
            set
            {
                VerifyCalledOnUIThread();
                if (_used != value)
                {
                    _used = value;
                    SendPropertyChanged("Used");

                }
            }
        }

        public PersonAccountModel(IFetchableModel<int> tracker)
        {
            _tracker = tracker;
            State = ModelState.Fetching;
            if (!ThreadPool.QueueUserWorkItem(new WaitCallback(FetchValueCallback)))
            {
                State = ModelState.Invalid;
            }
        }

        private void FetchValueCallback(object state)
        {
            int fetchedValue;
            if (_tracker.FetchData(out fetchedValue))
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ThreadStart(
                   delegate
                   {
                       Used = fetchedValue;
                       State = ModelState.Active;
                   }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ThreadStart(
                    delegate
                    {
                        State = ModelState.Invalid;
                    }));
            }
        }
    }
}
