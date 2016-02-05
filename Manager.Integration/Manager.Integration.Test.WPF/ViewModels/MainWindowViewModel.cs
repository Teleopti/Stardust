﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Manager.Integration.Test.WPF.Annotations;

namespace Manager.Integration.Test.WPF.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;

                OnPropertyChanged();
            }
        }


        public MainWindowViewModel()
        {
            GetData();

            RefreshTimer = new Timer(5000);

            RefreshTimer.Elapsed += RefreshTimerOnElapsed;

            RefreshTimer.Start();
        }

        private void RefreshTimerOnElapsed(object sender,
                                           ElapsedEventArgs elapsedEventArgs)
        {
            RefreshTimer.Stop();

            try
            {
                GetData();
            }

            finally
            {
                RefreshTimer.Start();
            }
        }

        public Timer RefreshTimer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                                    new PropertyChangedEventArgs(propertyName));
        }

        private List<Logging> _loggingData;

        public List<Logging> LoggingData
        {
            get { return _loggingData; }

            set
            {
                _loggingData = value;

                OnPropertyChanged();
            }
        }

        private void GetData()
        {
            Status = "Refresh started...";

            using (ManagerDbEntities managerDbEntities = new ManagerDbEntities())
            {
                LoggingData =
                    managerDbEntities.Loggings.OrderByDescending(logging => logging.Id)
                        .ToList();
            }

            Status = "Refresh finished.";
        }
    }
}