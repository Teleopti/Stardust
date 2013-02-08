using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.Model;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    public class ScheduleViewModel : INotifyPropertyChanged
    {
		private DateTime _startDate;
        private Visibility _resultCountVisible = Visibility.Hidden;
    	private GroupPageDto _selectedGroupPage;
    	private GroupPageGroupDto _selectedGroupPageGroup;

        public ScheduleViewModel()
        {
            FoundSchedules = new ObservableCollection<ScheduleModel>();
        	GroupPages = new ObservableCollection<GroupPageDto>();
        	GroupPageGroups = new ObservableCollection<GroupPageGroupDto>();

            var schedulingService = new ChannelFactory<ITeleoptiSchedulingService>(typeof(ITeleoptiSchedulingService).Name).CreateChannel();
			var organizationService = new ChannelFactory<ITeleoptiOrganizationService>(typeof(ITeleoptiOrganizationService).Name).CreateChannel();
            FindAll = new FindAllScheduleCommand(this, new PersonRepository(organizationService), new ScheduleRepository(schedulingService),
                                                 new ActivityRepository(schedulingService), new AbsenceRepository(schedulingService),
                                                 new OvertimeDefinitionSetRepository(organizationService));
			LoadGroupPages = new FindAllGroupPagesCommand(this, new GroupPageRepository(organizationService));
			LoadGroupPageGroups = new FindAvailableGroupPageGroupsCommand(this, new GroupPageRepository(organizationService));

            StartDate = DateTime.Today;
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

    	public Visibility ResultCountVisible
        {
            get { return _resultCountVisible; }
            set
            {
                _resultCountVisible = value;
                notifyPropertyChanged("ResultCountVisible");
            }
        }

    	public GroupPageDto SelectedGroupPage
    	{
    		get { return _selectedGroupPage; }
    		set
    		{
				_selectedGroupPage = value;
				notifyPropertyChanged("SelectedGroupPage");
    		}
    	}

		public GroupPageGroupDto SelectedGroupPageGroup
		{
			get { return _selectedGroupPageGroup; }
			set
			{
				_selectedGroupPageGroup = value;
				notifyPropertyChanged("SelectedGroupPageGroup");
			}
		}

    	public ICommand FindAll { get; private set; }

		public ICommand LoadGroupPages { get; private set; }

		public ICommand LoadGroupPageGroups { get; private set; }

        public ICollection<ScheduleModel> FoundSchedules
        {
            get; 
            private set;
        }

		public ICollection<GroupPageDto> GroupPages { get; private set; }

		public ICollection<GroupPageGroupDto> GroupPageGroups { get; private set; }

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
