using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadDayTemplatesPages
{
	public partial class WorkloadDayTemplatesDetailView : BaseUserControl
    {
        private readonly IWorkload _workload;
        private readonly int _templateIndex;
        private WorkloadIntradayTemplateGridControl _templateGridControl;
    	private bool _isDirty;
        private readonly IFilteredData _filteredDates = new FilteredData();
        private ButtonAdv _buttonFilterData;

        private IList<IWorkloadDayBase> _workloadDaysForTemplatesWithStatistics;
	    private FilterDataView _filterDataView;
		private readonly IStatisticHelper _statisticsHelper;
		internal event EventHandler<FilterDataViewClosedEventArgs> FilterDataViewClosed;

	    public WorkloadDayTemplatesDetailView(IStatisticHelper statisticsHelper)
        {
		    _statisticsHelper = statisticsHelper;
		    InitializeComponent();
            fillSmoothningBoxes();
            if(!DesignMode) 
                SetTexts();

			AddFilterButton();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void AddFilterButton()
		{
		    _buttonFilterData = new ButtonAdv
                                    {
		                                Anchor = AnchorStyles.Top,
		                                Appearance = ButtonAppearance.Metro,
		                                Name = "buttonFilterData",
		                                Size = new Size(141, 22),
                                        Text = UserTexts.Resources.FilterData,
                                        Enabled = false,
		                                UseVisualStyle = true,
										BackColor = Color.FromArgb(0,153,255)
		                            };
		                            
			_buttonFilterData.Click += btnFilterData_Click;

			dateSelectionComposite1.AddControlToLastRow(_buttonFilterData, 3);
		}

		private void btnFilterData_Click(object sender, EventArgs e)
		{
            //if (dateSelectionComposite1.SelectedDatesCount <= 0) return;
            _filterDataView = new FilterDataView(_workload, this, _templateIndex, _filteredDates);
            _filterDataView.InitializeStatistics(_workloadDaysForTemplatesWithStatistics);
			_filterDataView.Show(this);
		}

		public WorkloadDayTemplatesDetailView(IWorkload workload, int templateIndex, IStatisticHelper statisticsHelper)
            : this(statisticsHelper)
        {
            _workload = workload;
            _templateIndex = templateIndex;
            ReloadWorkloadDayTemplates();
            snapshotTemplateTaskList(TaskPeriodType.All);
            TextManager manager = new TextManager(_workload.Skill.SkillType);

            labelDeviationTasks.Text = manager.WordDictionary["DeviationCallsColon"];
            labelDeviationTaskTime.Text = manager.WordDictionary["DeviationTalkTimeColon"];
            labelDeviationAfterTaskTime.Text = manager.WordDictionary["DeviationACWColon"];

        }

        public WorkloadDayTemplatesDetailView(IWorkload workload, DayOfWeek dayOfWeek, IStatisticHelper statisticsHelper)
            : this(workload, (int)dayOfWeek, statisticsHelper)
        {
        }

        private void fillSmoothningBoxes()
        {
            IDictionary<int, object> runningNumbers = new Dictionary<int, object>();
            runningNumbers.Add(1,UserTexts.Resources.None);
            runningNumbers.Add(3,3);
            runningNumbers.Add(5,5);
            runningNumbers.Add(7,7);
            
            foreach (KeyValuePair<int, object> item in runningNumbers)
            {    
                cmbTaskRunningSmoothning.Items.Add(item);
                cmbTaskRunningSmoothning.DisplayMember = "Value";
                cmbAverageTimeRunningSmoothning.Items.Add(item);
                cmbAverageTimeRunningSmoothning.DisplayMember = "Value";                
                cmbAverageAfterCallWorkRunningSmoothning.Items.Add(item);
                cmbAverageAfterCallWorkRunningSmoothning.DisplayMember = "Value";
            }
            SetDefaultSmoothingParameters();
        }

	    private void SetDefaultSmoothingParameters()
	    {
	        cmbTaskRunningSmoothning.SelectedIndex = 0;
	        cmbAverageTimeRunningSmoothning.SelectedIndex = 0;
	        cmbAverageAfterCallWorkRunningSmoothning.SelectedIndex = 0;
	    }

	    public int TemplateIndex
	    {
            get { return _templateIndex; }
	    }

        /// <summary>
        /// Takes a Snapshots (copies the original task list to temp list)the template task list.
        /// For smoothtning use.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-27
        /// </remarks>
        private void snapshotTemplateTaskList(TaskPeriodType type)
        {
            IWorkloadDayTemplate template = (IWorkloadDayTemplate) _workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
            template.SnapshotTemplateTaskPeriodList(type);
        }

        public void ReloadWorkloadDayTemplates()
        {
            IWorkloadDayTemplate workloadDayTemplate = (IWorkloadDayTemplate) _workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);

            _templateGridControl = new WorkloadIntradayTemplateGridControl(workloadDayTemplate,
				new TaskOwnerHelper(new List<ITaskOwner> { workloadDayTemplate }), _workload.Skill.TimeZone, _workload.Skill.DefaultResolution, null, _workload.Skill.SkillType, _statisticsHelper);
            _templateGridControl.Create();

            _templateGridControl.ModifyCells += templateGridControl_ModifyCells;
            _templateGridControl.ModifyRows += templateGridControl_ModifyRows;
            GridToChart gridToChart = new GridToChart(_templateGridControl);
            gridToChart.Dock = DockStyle.Fill;
            gridToChart.Name = "DayTemplate";
            if (splitContainerMain.Panel1.Controls.Count > 0)
            {
                foreach (Control control in splitContainerMain.Panel1.Controls)
                {
                    control.Dispose();
                    splitContainerMain.Panel1.Controls.Remove(control);
                }
            }
            splitContainerMain.Panel1.Controls.Add(gridToChart);

            snapshotTemplateTaskList(TaskPeriodType.All);
            reApplySmoothing();
            if(_filterDataView !=null)
                ReloadFilterDataView();
        }

        private void ReloadFilterDataView()
        {
            _filterDataView.Reload();
        }

        void templateGridControl_ModifyRows(object sender, ModifyRowEventArgs e)
		{
			_isDirty = true;
			snapshotTemplateTaskList(e.RowType);
        }

        private void templateGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
			_isDirty = true;
            IList<ITemplateTaskPeriod> dataPeriods = e.DataPeriods.OfType<ITemplateTaskPeriod>().ToList();
            if (dataPeriods.Count < 1) return;

            WorkloadDayBase workloadDay = dataPeriods[0].Parent as WorkloadDayBase;
            if (workloadDay == null) return;

            switch (e.ModifyCellOption)
            {
                case ModifyCellOption.Merge:
                    if (dataPeriods.Count == 1) return;
                    workloadDay.MergeTemplateTaskPeriods(dataPeriods);
                    break;
                case ModifyCellOption.Split:
                    workloadDay.SplitTemplateTaskPeriods(dataPeriods);
                    break;
            }

            ITaskOwnerGrid grid = sender as ITaskOwnerGrid;
            if (grid == null) return;
            grid.RefreshGrid();

            snapshotTemplateTaskList(TaskPeriodType.All);
        }

        internal event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        private void cmbTaskRunningSmoothning_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ApplySmoothing(sender,TaskPeriodType.Tasks);
        }

        private void cmbAverageTimeRunningSmoothning_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ApplySmoothing(sender, TaskPeriodType.AverageTaskTime);
        }

        private void cmbAverageAfterCallWorkRunningSmoothning_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ApplySmoothing(sender, TaskPeriodType.AverageAfterTaskTime);
        }

        private void ApplySmoothing(object sender, TaskPeriodType taskPeriodType)
        {
            ComboBoxAdv comboBoxAdv = sender as ComboBoxAdv;
            if (comboBoxAdv == null || comboBoxAdv.SelectedItem == null) return;
            int periods = ((KeyValuePair<int, object>)comboBoxAdv.SelectedItem).Key;
            WorkloadDayTemplate template = (WorkloadDayTemplate)_workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
            template.DoRunningSmoothing(periods, taskPeriodType);
            _templateGridControl.UpdateGridRowToChart(taskPeriodType);
            Refresh();
        }

        private void reApplySmoothing()
        { 
            ApplySmoothing(cmbTaskRunningSmoothning, TaskPeriodType.Tasks);
            ApplySmoothing(cmbAverageTimeRunningSmoothning, TaskPeriodType.AverageTaskTime);
            ApplySmoothing(cmbAverageAfterCallWorkRunningSmoothning, TaskPeriodType.AverageAfterTaskTime);
        }

        public bool HasFilteredData()
        {
            if (dateSelectionComposite1.SelectedDatesCount != 0)
                return true;
            return false;
        }

        private void dateSelectionComposite1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            DateRangeChangedEventArgs dateRangeChangedEventArgs = new DateRangeChangedEventArgs(e.SelectedDates);
            DateRangeChanged(this, dateRangeChangedEventArgs);
        }

        private void splitContainerMain_DoubleClick(object sender, EventArgs e)
        {
            splitContainerMain.Panel2Collapsed = !splitContainerMain.Panel2Collapsed;
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

	    private void UnhookEvents()
        {
            _templateGridControl.ModifyRows -= templateGridControl_ModifyRows;
            _templateGridControl.ModifyCells -= templateGridControl_ModifyCells;
            dateSelectionComposite1.DateRangeChanged -= dateSelectionComposite1_DateRangeChanged;
            cmbAverageAfterCallWorkRunningSmoothning.SelectionChangeCommitted -=
                cmbAverageAfterCallWorkRunningSmoothning_SelectionChangeCommitted;
            cmbAverageTimeRunningSmoothning.SelectionChangeCommitted -=
                cmbAverageTimeRunningSmoothning_SelectionChangeCommitted;
            cmbTaskRunningSmoothning.SelectionChangeCommitted -= cmbTaskRunningSmoothning_SelectionChangeCommitted;
            splitContainerMain.DoubleClick -= splitContainerMain_DoubleClick;
        }

        private void ReleaseManagedResources()
        {
            if (_templateGridControl != null)
            {
                _templateGridControl.Dispose();
                _templateGridControl = null;
            	_buttonFilterData.Dispose();
				_buttonFilterData = null;
            }
        }

    	public void RefreshUpdatedDate()
    	{
			if (_isDirty)
			{
				var workloadDayTemplate = (IWorkloadDayTemplate) _workload.GetTemplateAt(TemplateTarget.Workload, _templateIndex);
				workloadDayTemplate.RefreshUpdatedDate();
			}
    	}

        internal void SetSelectedDates(IList<DateOnlyPeriod> selectedDates)
        {
            dateSelectionComposite1.SetSelectedDates(selectedDates);
        }

        public void UpdateFilteredWorkloadDays(IFilteredData filteredDates)
		{
		    Cursor.Current = Cursors.WaitCursor;
			
            _filteredDates.Merge(filteredDates);
            
			_filterDataView.Dispose();
            _filterDataView = null;
            TriggerFilterDataViewClosed(_filteredDates);
            
            Cursor.Current = Cursors.Default;
		}

	    public void EnableFilterData(bool state)
	    {
            _buttonFilterData.Enabled = state;
	    }

	    public void RefreshWorkloadDaysForTemplatesWithStatistics(IList<IWorkloadDayBase> historicalWorkloadDays)
	    {
	        _workloadDaysForTemplatesWithStatistics = historicalWorkloadDays;
	    }

        public void TriggerFilterDataViewClosed(IFilteredData filteredDates)
        {
        	var handler = FilterDataViewClosed;
            if (handler != null)
                handler(this, new FilterDataViewClosedEventArgs(filteredDates));
        }

		private void cmbTaskRunningSmoothning_Click(object sender, EventArgs e)
		{

		}
    }

    public class FilterDataViewClosedEventArgs : EventArgs
    {
        private readonly IFilteredData _filteredDates;

        public FilterDataViewClosedEventArgs(IFilteredData filteredDates)
        {
            _filteredDates = filteredDates;
        }

        public IFilteredData FilteredDates
        {
            get { return _filteredDates; }
        }
    }
}
