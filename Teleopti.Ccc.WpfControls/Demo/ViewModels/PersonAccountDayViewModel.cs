using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WpfControls.Demo.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Demo.ViewModels
{
    //TODO: Verify that public properties are only called on the UI-thread
    public class PersonAccountDayViewModel:DataModel
    {
        private PersonAccountDay _personAccount;
        private int _used;
        private int _calculatedTimes;
        private DayTransactionTracker _tracker;
        private ObservableCollection<DateTime> _tracks = new ObservableCollection<DateTime>();
        
      
        public int BalanceIn
        {
            get { return _personAccount.BalanceIn; }
            
        }

        public int CalculatedTimes
        {
            get { return _calculatedTimes; }
            set
            {
                if (_calculatedTimes!=value)
                {
                    _calculatedTimes = value;
                    SendPropertyChanged("CalculatedTimes");
                }
            }
        }

        public int Accrued
        {
            get { return _personAccount.Accrued; }
        }
        public int BalanceOut
        {
            get { return _personAccount.BalanceOut; }
        }

        public int Extra
        {
            get { return _personAccount.Extra; }
            set
            {
                if (_personAccount.Extra!=value)
                {
                    _personAccount.Extra = value;
                    SendPropertyChanged("Extra");
                }
                
            }
        }

        public DateTime StartDateTime
        {
            get { return _personAccount.StartDateTime; }
        }


        public ObservableCollection<DateTime> Tracks
        {
            get { return _tracks; }
        }
        public int Used
        {
            get { return _used; }
            set
            {
                if (_used!=value)
                {
                    _used = value;
                    SendPropertyChanged("Used");
                }
                
            }
        }

        public PersonAccountDayViewModel(PersonAccountDay personAccountDay):base()
        {
            _personAccount = personAccountDay;
            _tracker = new DayTransactionTracker();
            State = ModelState.Fetching;
            //if (!ThreadPool.QueueUserWorkItem(new WaitCallback(FetchValueCallback)))
            //{
            //    State = ModelState.Invalid;
            //}
        }

        public PersonAccountDayViewModel()
        {
            
        }

        public void GetData()
        {

            _tracker = new DayTransactionTracker();
            State = ModelState.Fetching;
            if (!ThreadPool.QueueUserWorkItem(new WaitCallback(FetchValueCallback)))
            {
                State = ModelState.Invalid;
               
            }
        }


        public void FetchValueCallback(object state)
        {

            CalculatedTimes++;
            int fetchedValue;
            if (_tracker.TryGetUsed(out fetchedValue,(IPerson)_personAccount.Parent,_personAccount.StartDateTime))
       
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ThreadStart(
                   delegate
                   {
                       
                       Used = fetchedValue;
                       _personAccount.CalculateBalanceIn();
                       _tracks = new ObservableCollection<DateTime>((List<DateTime>)_tracker.TrackedDateTimes);
                       SendPropertyChanged("BalanceIn");
                       SendPropertyChanged("BalanceOut");
                       SendPropertyChanged("Tracks");
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
