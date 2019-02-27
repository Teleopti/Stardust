using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadDayTemplatesPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class EditWorkloadDayTemplate : BaseDialogForm
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
		private readonly IWorkloadDay _workloadDay;
		private ReadOnlyCollection<DateOnlyPeriod> _selectedDates;
		private readonly IStatisticHelper _statisticsHelper;

		public EditWorkloadDayTemplate(IStatisticHelper statisticsHelper)
		{
			_statisticsHelper = statisticsHelper;
			InitializeComponent();
			if (!DesignMode) SetTexts();
			_toolTip.IsBalloon = true;
			_toolTip.InitialDelay = 1000;
			_toolTip.ToolTipTitle = UserTexts.Resources.InvalidAgentName;
		}

		public EditWorkloadDayTemplate(IWorkloadDayTemplate workloadDayTemplate, IStatisticHelper statHelper)
			: this(statHelper)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				initializeWorkload(uow, workloadDayTemplate.Workload);

				_workloadDayTemplate = _workload.TryFindTemplateByName(TemplateTarget.Workload, workloadDayTemplate.Name) as IWorkloadDayTemplate;
				if (_workloadDayTemplate != null) LazyLoadingManager.Initialize(_workloadDayTemplate.OpenHourList);
			}
			if (_workloadDayTemplate == null) return;

			if (!_workloadDayTemplate.OpenForWork.IsOpen)
			{
				closeTemplate();
			}
			else
			{
				_openHours = _workloadDayTemplate.OpenHourList[0];
			}
			_templateIndex = _workload.TemplateWeekCollection.First(t => _workloadDayTemplate.Equals(t.Value)).Key;
		}

		private void closeTemplate()
		{
			var midnightBreakOffset = _workload.Skill.MidnightBreakOffset;
			_openHours = new TimePeriod(midnightBreakOffset, midnightBreakOffset);
		}

		public EditWorkloadDayTemplate(IWorkload workload, IList<TimePeriod> openHours, IStatisticHelper statHelper)
			: this( statHelper)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				initializeWorkload(uow, workload);
			}
			_workloadDayTemplate = new WorkloadDayTemplate();
			if (openHours.Count > 0)
				_openHours = openHours[0];
			else
				_openHours = new TimePeriod(
					TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour), workload.Skill.DefaultResolution),
					TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour), workload.Skill.DefaultResolution)); //Must to this to enable users with higher resolution than one hour

			_workloadDayTemplate.Create("New Template", DateTime.UtcNow, _workload, new List<TimePeriod> { _openHours });
			textBoxTemplateName.ReadOnly = false;
			_templateIndex = _workload.AddTemplate(_workloadDayTemplate);
		}

		public EditWorkloadDayTemplate(IWorkloadDay workloadDay, IList<TimePeriod> openHours, IStatisticHelper statHelper)
			: this( statHelper)
		{
			_workloadDay = workloadDay;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				initializeWorkload(uow, workloadDay.Workload);
			}
			_workloadDayTemplate = new WorkloadDayTemplate();
			if (openHours.Count > 0)
				_openHours = openHours[0];
			else
				closeTemplate();

			_workloadDayTemplate.Create("New Template", DateTime.UtcNow, _workload, new List<TimePeriod> { _openHours });
			if (_workloadDay != null)
			{
				((WorkloadDayTemplate)_workloadDayTemplate).CloneTaskPeriodListFrom((WorkloadDayBase)_workloadDay);
				_workloadDayTemplate.ChangeOpenHours(new List<TimePeriod> { _openHours });
			}
			textBoxTemplateName.ReadOnly = false;
			_templateIndex = _workload.AddTemplate(_workloadDayTemplate);
		}

		private void initializeWorkload(IUnitOfWork unitOfWork, IWorkload workload)
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

		private void workloadDayTemplatesDetailViewDateRangeChanged(object sender, DateRangeChangedEventArgs e)
		{
			var workloadDayTemplatesDetailView = (WorkloadDayTemplatesDetailView)sender;
			_selectedDates = e.SelectedDates;
			List<IWorkloadDayBase> workloadDays;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var wr = new WorkloadDayTemplateCalculator(_statisticsHelper, OutlierRepository.DONT_USE_CTOR(uow));
				wr.RecalculateWorkloadDayTemplate(_selectedDates, _workload, _templateIndex);

				workloadDays = getWorkloadDaysForTemplatesWithStatistics(_statisticsHelper, _selectedDates);
			}
			workloadDayTemplatesDetailView.RefreshWorkloadDaysForTemplatesWithStatistics(workloadDays);
			workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
			workloadDayTemplatesDetailView.EnableFilterData(true);
			Refresh();
		}

		private List<IWorkloadDayBase> getWorkloadDaysForTemplatesWithStatistics(IStatisticHelper statisticHelper, IEnumerable<DateOnlyPeriod> selectedHistoricTemplatePeriod)
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
		private void buttonAdvEditOpenHoursClick(object sender, EventArgs e)
		{
			var isClosed = !(_openHours.SpanningTime() > TimeSpan.Zero);
			var openHourDialog = new OpenHourDialog(_openHours, _workload, false) {IsOpenHoursClosed = isClosed};
			if (openHourDialog.ShowDialog(this) == DialogResult.OK)
			{
				_openHours = openHourDialog.OpenHourPeriod;
				textBoxOpenHours.Text = _openHours.ToShortTimeString();
				var workloadDayTemplate = (WorkloadDayTemplate)_workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
				if (_workloadDay != null)
					workloadDayTemplate.CloneTaskPeriodListFrom((WorkloadDayBase)_workloadDay);
				workloadDayTemplate.ChangeOpenHours(new List<TimePeriod> { _openHours });
				_workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
		private void editTemplateLoad(object sender, EventArgs e)
		{
			_workloadDayTemplatesDetailView = new WorkloadDayTemplatesDetailView(_workload, _templateIndex, _statisticsHelper)
			{
				Dock = DockStyle.Fill
			};
			_workloadDayTemplatesDetailView.DateRangeChanged += workloadDayTemplatesDetailViewDateRangeChanged;
			_workloadDayTemplatesDetailView.FilterDataViewClosed += workloadDayTemplatesDetailViewFilterDataViewClosed;
			tableLayoutPanel1.Controls.Add(_workloadDayTemplatesDetailView, 0, 1);
			tableLayoutPanel1.SetColumnSpan(_workloadDayTemplatesDetailView, 2);

			textBoxTemplateName.Text = _workloadDayTemplate.Name;
			textBoxOpenHours.Text = _openHours.ToShortTimeString();

			_workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
		}

		private void workloadDayTemplatesDetailViewFilterDataViewClosed(object sender, FilterDataViewClosedEventArgs e)
		{
			reloadFilteredWorkloadDayTemplates(e.FilteredDates);
			_workloadDayTemplatesDetailView.ReloadWorkloadDayTemplates();
		}

		private void reloadFilteredWorkloadDayTemplates(IFilteredData filteredDates)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var wr = new WorkloadDayTemplateCalculator(_statisticsHelper, OutlierRepository.DONT_USE_CTOR(uow));
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
			if (textBoxTemplateName.Text == nameEmptyText || textBoxTemplateName.Text.Length >= 50) return false;

			var workloadDayTemplate = _workload.TryFindTemplateByName(TemplateTarget.Workload, textBoxTemplateName.Text) as IWorkloadDayTemplate;
			return (
					   workloadDayTemplate == null ||
					   workloadDayTemplate.Equals(_workloadDayTemplate));
		}

		private void textBoxTemplateNameTextChanged(object sender, EventArgs e)
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
				textBoxTemplateName.ForeColor = Color.Red;
				_toolTip.Hide(textBoxTemplateName);
				if (textBoxTemplateName.Text.Length >= 50)
				{
					_toolTip.Show(UserTexts.Resources.TheNameIsTooLong, textBoxTemplateName, new Point(textBoxTemplateName.Width - 30, -70), 5000);
				}
				else 
					if (textBoxTemplateName.Text != nameEmptyText)
				{
					_toolTip.Show(UserTexts.Resources.NameAlreadyExists, textBoxTemplateName, new Point(textBoxTemplateName.Width - 30, -70), 5000);
				}
			}
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public string TemplateName { get { return textBoxTemplateName.Text; } }

		private void buttonAdvOkClick(object sender, EventArgs e)
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
			EntityEventAggregator.TriggerEntitiesNeedRefresh(this, changes);
			var forecaster = Owner as Forecaster;
			if (forecaster != null) forecaster.RefreshTabs();

			DialogResult = DialogResult.OK;
			Close();
		}

		private void releaseManagedResources()
		{
			if (_workloadDayTemplatesDetailView != null)
			{
				_workloadDayTemplatesDetailView.DateRangeChanged -= workloadDayTemplatesDetailViewDateRangeChanged;
				_workloadDayTemplatesDetailView.FilterDataViewClosed -= workloadDayTemplatesDetailViewFilterDataViewClosed;
				_workloadDayTemplatesDetailView.Dispose();
			}
			_workloadDayTemplatesDetailView = null;
			_toolTip.Dispose();
		}
	}
}
