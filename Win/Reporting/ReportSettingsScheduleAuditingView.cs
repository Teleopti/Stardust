﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Presentation;

namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportSettingsScheduleAuditingView : BaseUserControl, IReportSettingsScheduleAuditingView
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IComponentContext _componentContext;
        private readonly IApplicationFunction _applicationFunction;
        private readonly ReportSettingsScheduleAuditingPresenter _presenter;
        private const int numberOfDays = 1;
        //  fetch from last saved data
        private HashSet<Guid> _selectedAgentGuids = new HashSet<Guid>();
        private ReportSettingsScheduleAuditing _setting;

        public ReportSettingsScheduleAuditingView(IEventAggregator eventAggregator, IComponentContext componentContext, IApplicationFunction applicationFunction)
        {
            _eventAggregator = eventAggregator;
            _componentContext = componentContext;
            _applicationFunction = applicationFunction;
            InitializeComponent();

            if (!StateHolderReader.IsInitialized || DesignMode) return;

            _presenter = new ReportSettingsScheduleAuditingPresenter(this);
            reportDateFromToSelectorSchedulePeriod.PeriodChanged += reportDateFromToSelectorSchedulePeriodPeriodChanged;
            _eventAggregator.GetEvent<LoadReportDone>().Subscribe(onLoadReportDone);
            
        }

        private void onLoadReportDone(bool obj)
        {
            Enabled = true;
        }

        void reportDateFromToSelectorSchedulePeriodPeriodChanged(object sender, EventArgs e)
        {
            setPersonSelectorDate();
        }
        
        private void loadSetting()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _setting = new PersonalSettingDataRepository(uow).FindValueByKey("ReportSettingsScheduleAuditing", new ReportSettingsScheduleAuditing());
                _selectedAgentGuids = _setting.Agents;
            }

        }

        private void setPersonSelectorDate()
        {
            reportPersonSelector1.SetPeriod(reportDateFromToSelectorSchedulePeriod.GetSelectedDates.First());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!StateHolderReader.IsInitialized || DesignMode) return;

            SetTexts();

            reportDateFromToSelectorChangePeriod.WorkPeriodStart = _setting.ChangeStartDate;
            reportDateFromToSelectorChangePeriod.WorkPeriodEnd = _setting.ChangeEndDate;

            reportDateFromToSelectorSchedulePeriod.WorkPeriodStart = _setting.ScheduleStartDate;
            reportDateFromToSelectorSchedulePeriod.WorkPeriodEnd = _setting.ScheduleEndDate;
            reportUserSelectorAuditingView1.SetSelectedUserById(_setting.User);
        }

        public DateOnlyPeriod SchedulePeriod
        {
            get
            {
                var adjustedPeriod = changeEndDate(reportDateFromToSelectorSchedulePeriod.GetSelectedDates.First());
                return adjustedPeriod;
            }
        }

        public void InitUserSelector()
        {
            reportUserSelectorAuditingView1.Initialize();
        }

        public void InitPersonSelector()
        {
            reportPersonSelector1.Init(_selectedAgentGuids, _componentContext, _applicationFunction, _setting.GroupPage); 
        }

        public void SetDateControlTexts()
        {
            reportDateFromToSelectorChangePeriod.SetDateFromLabelText(Resources.ChangeDateFromColon);
            reportDateFromToSelectorChangePeriod.SetDateToLabelText(Resources.ChangeDateToColon);
            reportDateFromToSelectorSchedulePeriod.SetDateFromLabelText(Resources.ScheduleDateFromColon);
            reportDateFromToSelectorSchedulePeriod.SetDateToLabelText(Resources.ScheduleDateToColon);
        }

        public void InitializeSettings()
        {
            loadSetting();
            _presenter.InitializeSettings();
        }

        public IList<IPerson> ModifiedBy
        {
            get { return reportUserSelectorAuditingView1.SelectedUsers; }
        }

        public DateOnlyPeriod ChangePeriod
        {
            get
            {
                var adjustedPeriod = changeEndDate(reportDateFromToSelectorChangePeriod.GetSelectedDates.First());
                return adjustedPeriod;
            }
        }

        private void buttonAdvOkClick(object sender, EventArgs e)
        {
            Enabled = false;
            try
            {
                saveSetting();
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }

                Enabled = true;
                return;
            }

            _eventAggregator.GetEvent<LoadReport>().Publish(true);
        }

        private void saveSetting()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _setting.User = reportUserSelectorAuditingView1.SelectedUserModel.Id;
                
                 _setting.Agents = reportPersonSelector1.SelectedAgentGuids;

                _setting.ChangeStartDate = reportDateFromToSelectorChangePeriod.WorkPeriodStart;
                _setting.ChangeEndDate = reportDateFromToSelectorChangePeriod.WorkPeriodEnd;

                _setting.ScheduleStartDate = reportDateFromToSelectorSchedulePeriod.WorkPeriodStart;
                _setting.ScheduleEndDate = reportDateFromToSelectorSchedulePeriod.WorkPeriodEnd;
                _setting.GroupPage = reportPersonSelector1.SelectedGroupPage();
                new PersonalSettingDataRepository(uow).PersistSettingValue(_setting);
                uow.PersistAll();
            }
        }

        public ICollection<IPerson> Agents
        {
            get { return reportPersonSelector1.SelectedAgents; }
        }

        public ReportSettingsScheduleAuditingModel ScheduleAuditingSettingsModel
        {
            get { return _presenter.GetSettingsModel; }
        }

        private static DateOnlyPeriod changeEndDate(DateOnlyPeriod dateOnlyPeriod)
        {
            return new DateOnlyPeriod(dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate.AddDays(numberOfDays));   
        }


        public DateOnlyPeriod ChangePeriodDisplay
        {
            get { return reportDateFromToSelectorChangePeriod.GetSelectedDates.First(); }
        }

        public DateOnlyPeriod SchedulePeriodDisplay
        {
            get { return reportDateFromToSelectorSchedulePeriod.GetSelectedDates.First(); }
        }

    }
}
