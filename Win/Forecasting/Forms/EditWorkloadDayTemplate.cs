﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.Forecasting.Forms.WorkloadDayTemplatesPages;
using Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public partial class EditWorkloadDayTemplate : BaseRibbonForm
    {
        private IWorkload _workload;
        private IWorkload _originalWorkload;
        private TimePeriod _openHours;
        private readonly int _templateIndex;
        private readonly IWorkloadDayTemplate _workloadDayTemplate;
        private WorkloadDayTemplatesDetailView _workloadDayTemplatesDetailView;
        private readonly string nameEmptyText = UserTexts.Resources.EnterNameHere;
        private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
        private readonly ToolTip _toolTip = new ToolTip();
        private IWorkloadDay _workloadDay;
        private ReadOnlyCollection<DateOnlyPeriod> _selectedDates;

        public EditWorkloadDayTemplate()
        {
            InitializeComponent();
            SetColor();
            if (!DesignMode) SetTexts();
            _toolTip.IsBalloon = true;
            _toolTip.InitialDelay = 1000;
            _toolTip.ToolTipTitle = UserTexts.Resources.InvalidAgentName;
        }

        private void SetColor()
        {
            BrushInfo panelbrush = ColorHelper.ControlGradientPanelBrush();
            gradientPanel1.BackgroundColor = panelbrush;
        }

        public EditWorkloadDayTemplate(IWorkloadDayTemplate workloadDayTemplate)
            : this()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InitializeWorkload(uow, workloadDayTemplate.Workload);

                _workloadDayTemplate = _workload.TryFindTemplateByName(TemplateTarget.Workload, workloadDayTemplate.Name) as IWorkloadDayTemplate;
                if (_workloadDayTemplate != null) LazyLoadingManager.Initialize(_workloadDayTemplate.OpenHourList);
            }
            if (_workloadDayTemplate == null) return;

            if (!_workloadDayTemplate.OpenForWork.IsOpen)
            {
                CloseTemplate();
            }
            else
            {
                _openHours = _workloadDayTemplate.OpenHourList[0];
            }
            _templateIndex = (from t in _workload.TemplateWeekCollection
                              where t.Value == _workloadDayTemplate
                              select t.Key).First();
        }

        private void CloseTemplate()
        {
            var midnightBreakOffset = _workload.Skill.MidnightBreakOffset;
            _openHours = new TimePeriod(midnightBreakOffset, midnightBreakOffset);
        }

        public EditWorkloadDayTemplate(IWorkload workload, IList<TimePeriod> openHours)
            : this()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InitializeWorkload(uow, workload);
            }
            _workloadDayTemplate = new WorkloadDayTemplate();
            if (openHours.Count > 0)
                _openHours = openHours[0];
            else
                _openHours = new TimePeriod(
                    TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(8), workload.Skill.DefaultResolution),
                    TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(17), workload.Skill.DefaultResolution)); //Must to this to enable users with higher resolution than one hour

            _workloadDayTemplate.Create("New Template", DateTime.UtcNow, _workload, new List<TimePeriod> { _openHours });
            textBoxTemplateName.ReadOnly = false;
            _templateIndex = _workload.AddTemplate(_workloadDayTemplate);
        }

        public EditWorkloadDayTemplate(IWorkloadDay workloadDay, IList<TimePeriod> openHours)
            : this()
        {
            _workloadDay = workloadDay;
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InitializeWorkload(uow, workloadDay.Workload);
            }
            _workloadDayTemplate = new WorkloadDayTemplate();
            if (openHours.Count > 0)
                _openHours = openHours[0];
            else
                CloseTemplate();

            _workloadDayTemplate.Create("New Template", DateTime.UtcNow, _workload, new List<TimePeriod> { _openHours });
            if (_workloadDay != null)
            {
                ((WorkloadDayTemplate)_workloadDayTemplate).CloneTaskPeriodListFrom((WorkloadDayBase)_workloadDay);
                _workloadDayTemplate.ChangeOpenHours(new List<TimePeriod> { _openHours });
            }
            textBoxTemplateName.ReadOnly = false;
            _templateIndex = _workload.AddTemplate(_workloadDayTemplate);
        }

        private void InitializeWorkload(IUnitOfWork unitOfWork, IWorkload workload)
        {
            IWorkloadRepository workloadRepository = _repositoryFactory.CreateWorkloadRepository(unitOfWork);
            _originalWorkload = workloadRepository.Load(workload.Id.Value);
            LazyLoadingManager.Initialize(_originalWorkload.Skill);
            LazyLoadingManager.Initialize(_originalWorkload.Skill.TemplateWeekCollection);
            LazyLoadingManager.Initialize(_originalWorkload.Skill.SkillType);
            LazyLoadingManager.Initialize(_originalWorkload.TemplateWeekCollection);
            LazyLoadingManager.Initialize(_originalWorkload.QueueSourceCollection);

            foreach (KeyValuePair<int, ISkillDayTemplate> valuePair in _originalWorkload.Skill.TemplateWeekCollection)
            {
                LazyLoadingManager.Initialize(valuePair.Value.TemplateSkillDataPeriodCollection);
            }
            _originalWorkload.Skill.EntityClone();
            _workload = _originalWorkload.EntityClone();
        }

        private void workloadDayTemplatesDetailView_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            WorkloadDayTemplatesDetailView workloadDayTemplatesDetailView = (WorkloadDayTemplatesDetailView)sender;
            _selectedDates = e.SelectedDates;
            var workloadDays = new List<IWorkloadDayBase>();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                StatisticHelper statisticHelper = new StatisticHelper(
                    _repositoryFactory, uow);

                var wr = new WorkloadDayTemplateCalculator(statisticHelper, new OutlierRepository(uow));
                wr.RecalculateWorkloadDayTemplate(_selectedDates, _workload, _templateIndex);

                workloadDays = GetWorkloadDaysForTemplatesWithStatistics(statisticHelper, _selectedDates);
            }
            workloadDayTemplatesDetailView.RefreshWorkloadDaysForTemplatesWithStatistics(workloadDays);
            workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
            workloadDayTemplatesDetailView.EnableFilterData(true);
            Refresh();
        }

        private List<IWorkloadDayBase> GetWorkloadDaysForTemplatesWithStatistics(StatisticHelper statisticHelper, IEnumerable<DateOnlyPeriod> selectedHistoricTemplatePeriod)
        {
            var workloadDays = new List<IWorkloadDayBase>();
            foreach (var period in selectedHistoricTemplatePeriod)
            {
                var statisticData = statisticHelper.LoadStatisticData(period, _workload);
                workloadDays.AddRange(statisticData);
            }
            return workloadDays;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        private void buttonAdvEditOpenHours_Click(object sender, EventArgs e)
        {
            var isClosed = !(_openHours.SpanningTime() > TimeSpan.Zero);
            var openHourDialog = new OpenHourDialog(_openHours, _workload, false) {IsOpenHoursClosed = isClosed};
            if (openHourDialog.ShowDialog(this) == DialogResult.OK)
            {
                _openHours = openHourDialog.OpenHourPeriod;
                if (_openHours.EndTime.Days > 0)
                    textBoxOpenHours.Text = _openHours.ToShortTimeString() + " +1";
                else
                    textBoxOpenHours.Text = _openHours.ToShortTimeString();
                var workloadDayTemplate = (WorkloadDayTemplate)_workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
                if (_workloadDay != null)
                    workloadDayTemplate.CloneTaskPeriodListFrom((WorkloadDayBase)_workloadDay);
                workloadDayTemplate.ChangeOpenHours(new List<TimePeriod> { _openHours });
                _workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        private void EditTemplate_Load(object sender, EventArgs e)
        {
            _workloadDayTemplatesDetailView = new WorkloadDayTemplatesDetailView(_workload, _templateIndex);
            _workloadDayTemplatesDetailView.Dock = DockStyle.Fill;
            _workloadDayTemplatesDetailView.DateRangeChanged += workloadDayTemplatesDetailView_DateRangeChanged;
            _workloadDayTemplatesDetailView.FilterDataViewClosed += workloadDayTemplatesDetailView_FilterDataViewClosed;
            tableLayoutPanel1.Controls.Add(_workloadDayTemplatesDetailView, 0, 1);
            tableLayoutPanel1.SetColumnSpan(_workloadDayTemplatesDetailView, 2);

            textBoxTemplateName.Text = _workloadDayTemplate.Name;
            if (_openHours.EndTime.Days > 0)
                textBoxOpenHours.Text = _openHours.ToShortTimeString() + " +1";
            else
                textBoxOpenHours.Text = _openHours.ToShortTimeString();

            _workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
        }

        private void workloadDayTemplatesDetailView_FilterDataViewClosed(object sender, FilterDataViewClosedEventArgs e)
        {
            ReloadFilteredWorkloadDayTemplates(e.FilteredDates);
            _workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
        }

        private void ReloadFilteredWorkloadDayTemplates(IFilteredData filteredDates)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var statHelper = new StatisticHelper(new RepositoryFactory(), uow);
                var wr = new WorkloadDayTemplateCalculator(statHelper, new OutlierRepository(uow));
                wr.RecalculateWorkloadDayTemplate(_selectedDates, _workload, _templateIndex, filteredDates.FilteredDateList());
            }
        }

        private bool nameIsValid()
        {
            if (String.IsNullOrEmpty(textBoxTemplateName.Text.Trim()))
            {
                textBoxTemplateName.Text = nameEmptyText;
                textBoxTemplateName.SelectAll();
            }
            if (textBoxTemplateName.Text == nameEmptyText) return false;

            IWorkloadDayTemplate workloadDayTemplate;
            workloadDayTemplate = _workload.TryFindTemplateByName(TemplateTarget.Workload, textBoxTemplateName.Text) as IWorkloadDayTemplate;
            return (
                       workloadDayTemplate == null ||
                       workloadDayTemplate.Equals(_workloadDayTemplate));
        }

        private void textBoxTemplateName_TextChanged(object sender, EventArgs e)
        {
            if (nameIsValid())
            {
                buttonAdvOK.Enabled = true;
                textBoxTemplateName.ForeColor = Color.FromKnownColor(KnownColor.WindowText);

                _toolTip.Hide(textBoxTemplateName);
            }
            else
            {
                buttonAdvOK.Enabled = false;
                if (textBoxTemplateName.Text != nameEmptyText)
                {
                    textBoxTemplateName.ForeColor = Color.Red;
                    _toolTip.Show(UserTexts.Resources.NameAlreadyExists, textBoxTemplateName, new Point(textBoxTemplateName.Width - 30, -70), 5000);
                }
            }
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public string TemplateName { get { return textBoxTemplateName.Text; } }

        private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            _workloadDayTemplate.Name = textBoxTemplateName.Text;
            _workloadDayTemplate.RefreshUpdatedDate();
            IEnumerable<IRootChangeInfo> changes;
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                uow.Reassociate(_originalWorkload);
                uow.Merge(_workload);
                changes = uow.PersistAll();
            }
            Main.EntityEventAggregator.TriggerEntitiesNeedRefresh(this, changes);
            Forecaster forecaster = Owner as Forecaster;
            if (forecaster != null) forecaster.RefreshTabs();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ReleaseManagedResources()
        {
            if (_workloadDayTemplatesDetailView != null)
            {
                _workloadDayTemplatesDetailView.DateRangeChanged -= workloadDayTemplatesDetailView_DateRangeChanged;
                _workloadDayTemplatesDetailView.FilterDataViewClosed -= workloadDayTemplatesDetailView_FilterDataViewClosed;
                _workloadDayTemplatesDetailView.Dispose();
            }
            _workloadDayTemplatesDetailView = null;
			_toolTip.Dispose();
        }

    }
}
