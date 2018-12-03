using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AddLayerViewModel<T> : DataModel, IAddLayerViewModel<T> where T : class
    {
        private readonly ReadOnlyCollection<T> _payloads;
        private bool _showDetails;
        private bool _canOk;

        public DateTimePeriodViewModel PeriodViewModel { get; private set; }

        public AddLayerViewModel(IEnumerable<T> payloads, DateTimePeriod period, string title, TimeSpan interval, T defaultPayload)
        {
            PeriodViewModel = new DateTimePeriodViewModel();
            PeriodViewModel.Start = period.StartDateTime;
            PeriodViewModel.End = period.EndDateTime;
            PeriodViewModel.Interval = interval;
            PeriodViewModel.AutoUpdate = false;
            Title = title;
            _payloads = payloads.ToList().AsReadOnly();
            ShowDetailsToggleCommand = CommandModelFactory.CreateCommandModel(() => { ShowDetails = !ShowDetails; },
                                                                            CommonRoutedCommands.ShowDetails);
            CanOk = true;

			if(defaultPayload != null)
				Payloads.MoveCurrentTo(defaultPayload);
        }

	    public AddLayerViewModel(IEnumerable<T> payloads, ISetupDateTimePeriod setupDateTimePeriod, string title,
		    TimeSpan interval) : this(payloads, setupDateTimePeriod.Period, title, interval, null)
	    {
		    //Todo, set all locks and stuff from ISetupDateTimePeriod
	    }

	    public bool Result
        {
            get; set;
        }

        public  string Title { get; private set;}
     
        public DateTimePeriod SelectedPeriod
        {
            get { return new DateTimePeriod(PeriodViewModel.Start, PeriodViewModel.End); }
        }

        public ICollectionView Payloads
        {
            get { return CollectionViewSource.GetDefaultView(_payloads) as ListCollectionView; }
        }

        public T SelectedItem
        {
            get
            {
                return Payloads.CurrentItem as T;
            }
        }

        public bool ShowDetails
        {
            get { return _showDetails; }
            private set
            {
                if(_showDetails!=value)
                {
                    _showDetails = value;
                    SendPropertyChanged(nameof(ShowDetails));
                }
            }
        }

        /// <summary>
        /// Toggles the advnced stetings on or off
        /// </summary>
        /// <value>The toggle advanced settings.</value>
        public CommandModel ShowDetailsToggleCommand
        {
            get; 
            private set; 
        }

        public bool CanOk
        {
            get
            {
                return _canOk;
            }
            protected set
            {
                if(_canOk!=value)
                {
                    _canOk = value;
                    SendPropertyChanged(nameof(CanOk));
                }
            }
        }
    }
}