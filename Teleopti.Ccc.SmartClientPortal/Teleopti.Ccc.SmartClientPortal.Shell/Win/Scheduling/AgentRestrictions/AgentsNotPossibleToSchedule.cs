using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public partial class AgentsNotPossibleToSchedule : UserControl
	{
		private RestrictionNotAbleToBeScheduledReport _restrictionNotAbleToBeScheduledReport;
		private IEnumerable<IPerson> _selectedAgents;
		private DateOnly _selectedDate;
		private IEnumerable<RestrictionsNotAbleToBeScheduledResult> _result;
		private SchedulerSplitters _parent;

		public AgentsNotPossibleToSchedule()
		{
			InitializeComponent();
		}

		public void InitAgentsNotPossibleToSchedule(RestrictionNotAbleToBeScheduledReport restrictionNotAbleToBeScheduledReport, SchedulerSplitters parent)
		{
			_restrictionNotAbleToBeScheduledReport = restrictionNotAbleToBeScheduledReport;
			_parent = parent;
		}

		private void toolStripButtonRefreshClick(object sender, EventArgs e)
		{
			toolStripButtonRefresh.Enabled = false;
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();
			listViewResult.ResumeLayout(true);
			backgroundWorker1.RunWorkerAsync();
		}

		public void SetSelected(IEnumerable<IPerson> selectedAgents, DateOnly selectedDate)
		{
			_selectedAgents = selectedAgents;
			_selectedDate = selectedDate;
		}

		private void backgroundWorker1DoWork(object sender, DoWorkEventArgs e)
		{
			backgroundWorker1.ReportProgress(0, "XXAnalyzingRestrictions");
			_result = _restrictionNotAbleToBeScheduledReport.Create(_selectedDate,
				_selectedAgents, new BackgroundWorkerWrapper(backgroundWorker1));
		}

		private void backgroundWorker1ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_parent.OnRestrictionsNotAbleToBeScheduledProgress(e);
		}

		private void backgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_parent.OnRestrictionsNotAbleToBeScheduledProgress(new ProgressChangedEventArgs(100, ""));
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();

			foreach (var restrictionsAbleToBeScheduledResult in _result)
			{
				var item = new ListViewItem(restrictionsAbleToBeScheduledResult.Agent.Name.ToString());
				item.SubItems.Add(restrictionsAbleToBeScheduledResult.Reason.ToString());
				item.SubItems.Add(restrictionsAbleToBeScheduledResult.Period.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
				listViewResult.Items.Add(item);
			}

			listViewResult.ResumeLayout(true);
			toolStripButtonRefresh.Enabled = true;
		}
	}
}
