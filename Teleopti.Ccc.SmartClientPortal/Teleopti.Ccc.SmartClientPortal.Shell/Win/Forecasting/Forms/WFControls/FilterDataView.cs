using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadDayTemplatesPages;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
	public partial class FilterDataView : BaseDialogForm, INotifyWorkloadDayFiltered
	{
		private readonly IWorkload _workload;
		private readonly WorkloadDayTemplatesDetailView _owner;
		private readonly int _templateIndex;
		private readonly IFilteredData _filteredDates = new FilteredData();
		private WorkloadDayTemplateFilterStatus _filterStatus;
		private WorkloadDayTemplate _workloadDayTemplate;
		private IList<IWorkloadDayBase> _workloadDaysForTemplatesWithStatistics;
		private FilterDataDetailView _detailView;

		public FilterDataView(IWorkload workload, WorkloadDayTemplatesDetailView owner, int templateIndex, IFilteredData filteredDates)
		{
			_workload = workload;
			_owner = owner;
			_templateIndex = templateIndex;

			initializeFilteredDates(filteredDates);
			
			InitializeComponent();
			SetTexts();
		}

		public void InitializeStatistics(IList<IWorkloadDayBase> workloadDaysForTemplatesWithStatistics)
		{
			_workloadDaysForTemplatesWithStatistics = workloadDaysForTemplatesWithStatistics;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Reload();
		}

		private void initializeFilteredDates(IFilteredData filteredDates)
		{
			if (filteredDates == null) throw new ArgumentNullException("filteredDates");
			_filteredDates.Merge(filteredDates);
		}
	
		private void btnOkClick(object sender, EventArgs e)
		{
			Close();
			_owner.UpdateFilteredWorkloadDays(_filteredDates);
		}

		private void btnCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		public void NotifyWorkloadDayFilterChanged(DateOnly filteredDate, bool checkValue)
		{
			_filteredDates.AddOrUpdate(filteredDate, checkValue);
		}

		private void backgroundWorker1DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var workloadDayDaysWithFilterStatus = new List<WorkloadDayWithFilterStatus>();

			List<IWorkloadDayBase> tempVisibleWorkloadDays;

			if(_templateIndex < 7 )
				tempVisibleWorkloadDays = _workloadDaysForTemplatesWithStatistics.Where(workloadDay => (int)workloadDay.CurrentDate.DayOfWeek == _templateIndex).OrderBy(d => d.CurrentDate).ToList();
			else
				tempVisibleWorkloadDays = _workloadDaysForTemplatesWithStatistics.ToList();

			var visibleWorkloadDays = new List<IWorkloadDayBase>();
			//unique days
			foreach (var workloadDay in tempVisibleWorkloadDays)
			{
				visibleWorkloadDays.Add(workloadDay);
			}
			//template column
			_workloadDayTemplate = new WorkloadDayTemplate();
			if (Enum.IsDefined(typeof(DayOfWeek),_templateIndex))
				_workloadDayTemplate.Create("Template", DateTime.UtcNow, _workload,((IWorkloadDayTemplate)_workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex)).OpenHourList);
			else
				_workloadDayTemplate = (WorkloadDayTemplate) _workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
			foreach (var day in visibleWorkloadDays.OfType<IWorkloadDay>())
			{
				day.ApplyTemplate(_workloadDayTemplate, workloadDay => workloadDay.Lock(), workloadDay => workloadDay.Release());
			}
			var templateWorkloadDay = new Statistic(_workload).GetTemplateWorkloadDay(_workloadDayTemplate, visibleWorkloadDays);
			workloadDayDaysWithFilterStatus.Add(new WorkloadDayWithFilterStatus(templateWorkloadDay, true, this));
			foreach (var workloadDay in visibleWorkloadDays)
			{
				var currentDate = workloadDay.CurrentDate;
				if(!_filteredDates.Contains(currentDate)) _filteredDates.AddOrUpdate(currentDate, true);
				workloadDayDaysWithFilterStatus.Add(new WorkloadDayWithFilterStatus(workloadDay,
																					_filteredDates.FilteredDates[currentDate],
																					this));
			}
			_filterStatus = new WorkloadDayTemplateFilterStatus
			{
				WorkloadDaysWithFilterStatus = workloadDayDaysWithFilterStatus,
				TemplateIndex = _templateIndex,
			};
		   
		}

		private void backgroundWorker1RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			_detailView = new FilterDataDetailView(_filterStatus, _workload, _workloadDayTemplate) { Dock = DockStyle.Fill };
			pictureBoxLoading.SendToBack();
			gradientPanelMain.Visible = true;
			tableLayoutPanel1.Controls.Add(_detailView, 0, 0);
			tableLayoutPanel1.SetColumnSpan(_detailView, 3);
			btnOk.Visible = true;
			btnCancel.Visible = true;
			Cursor = Cursors.Default;
		}

		public void Reload()
		{
			gradientPanelMain.Visible = false;
			removeExistingDetailView();
			pictureBoxLoading.BringToFront();
			if (!backgroundWorker1.IsBusy)
				backgroundWorker1.RunWorkerAsync();
		}

		private void removeExistingDetailView()
		{
			if (_detailView != null)
			{
				tableLayoutPanel1.Controls.Remove(_detailView);
				_detailView = null;
			}
		}
	}

	public class WorkloadDayWithFilterStatus
	{
		private IWorkloadDayBase _workloadDay;
		private bool _included;
		private readonly INotifyWorkloadDayFiltered _notifier;

		public WorkloadDayWithFilterStatus(IWorkloadDayBase workloadDay, bool included, INotifyWorkloadDayFiltered notifier)
		{
			_workloadDay = workloadDay;
			_included = included;
			_notifier = notifier;
		}

		public bool Included
		{
			get { return _included; }
			set
			{
				if (_included == value) return;
				_included = value;
				_notifier.NotifyWorkloadDayFilterChanged(_workloadDay.CurrentDate, _included);
			}
		}

		public IWorkloadDayBase WorkloadDay
		{
			get { return _workloadDay; }
			set { _workloadDay = value; }
		}
	}

	public class WorkloadDayTemplateFilterStatus
	{
		public IEnumerable<WorkloadDayWithFilterStatus> WorkloadDaysWithFilterStatus { get; set; }

		public int TemplateIndex { get; set; }
	}

	public interface INotifyWorkloadDayFiltered
	{
		void NotifyWorkloadDayFilterChanged(DateOnly filteredDate, bool checkValue);
	}
}
