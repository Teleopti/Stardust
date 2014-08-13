using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
	public partial class FilterDataDetailView : BaseUserControl
	{
		private readonly WorkloadDayTemplateFilterStatus _filterStatus;
		private readonly IWorkload _workload;
		private readonly IWorkloadDayTemplate _workloadDayTemplate;
		private WorkloadDayFilterGridControl _workloadDayFilterControl;

		public FilterDataDetailView()
		{
			InitializeComponent();
			if(!DesignMode) 
				SetTexts();
		}

		public FilterDataDetailView(WorkloadDayTemplateFilterStatus filterStatus, IWorkload workload, IWorkloadDayTemplate workloadDayTemplate) : this()
		{
			_filterStatus = filterStatus;
			_workload = workload;
			_workloadDayTemplate = workloadDayTemplate;
			Initialize();
		}

		public void Initialize()
		{
			_workloadDayFilterControl = new WorkloadDayFilterGridControl(_workload, _workloadDayTemplate);
			_workloadDayFilterControl.LoadTaskOwnerDaysWithFilterStatus(_filterStatus);
			var gridToChart = new FilterGridToChart(_workloadDayFilterControl) {Dock = DockStyle.Fill, Name = "DayTemplate"};
			_workloadDayFilterControl.InitializeChart();
			
			tableLayoutPanel1.Controls.Add(gridToChart);
			_workloadDayFilterControl.Refresh();
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		private void releaseManagedResources()
		{
			if (_workloadDayFilterControl != null)
			{
				_workloadDayFilterControl.Dispose();
				_workloadDayFilterControl = null;
			}
		}

	}
}
