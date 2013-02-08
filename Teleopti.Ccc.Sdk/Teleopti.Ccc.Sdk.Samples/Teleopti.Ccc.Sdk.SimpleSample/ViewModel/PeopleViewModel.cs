using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.SimpleSample.Model;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    public class PeopleViewModel : INotifyPropertyChanged
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private Visibility _resultCountVisible = Visibility.Hidden;

        public PeopleViewModel()
		{
			var organizationService = new ChannelFactory<ITeleoptiOrganizationService>(typeof(ITeleoptiOrganizationService).Name).CreateChannel();
			var forecastingService = new ChannelFactory<ITeleoptiForecastingService>(typeof(ITeleoptiForecastingService).Name).CreateChannel();
            

            FoundPeople = new ObservableCollection<PersonDetailModel>();
			FindAll = new FindAllPeopleCommand(this, new PersonRepository(organizationService), new ContractRepository(organizationService), new PartTimePercentageRepository(organizationService), new ContractScheduleRepository(organizationService), new SkillRepository(forecastingService), new BusinessHierarchyRepository(organizationService), new PersonPeriodRepository(organizationService));
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                notifyPropertyChanged("StartDate");
            }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                notifyPropertyChanged("EndDate");
            }
        }

        public Visibility ResultCountVisible
        {
            get { return _resultCountVisible; }
            set
            {
                _resultCountVisible = value;
                notifyPropertyChanged("ResultCountVisible");
            }
        }

        public ICommand FindAll { get; private set; }

        public ICollection<PersonDetailModel> FoundPeople
        {
            get; 
            private set;
        } 

        protected virtual void notifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler!=null)
            {
                handler.Invoke(this,new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
