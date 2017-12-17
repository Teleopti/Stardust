using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public partial class AgentsNotPossibleToSchedule : UserControl
	{
		private RestrictionNotAbleToBeScheduledReport _restrictionNotAbleToBeScheduledReport;
		private IEnumerable<IPerson> _selectedAgents;
		private DateOnly _selectedDate;

		public AgentsNotPossibleToSchedule()
		{
			InitializeComponent();
		}

		public void InitAgentsNotPossibleToSchedule(RestrictionNotAbleToBeScheduledReport restrictionNotAbleToBeScheduledReport)
		{
			_restrictionNotAbleToBeScheduledReport = restrictionNotAbleToBeScheduledReport;
		}

		private void toolStripButtonRefreshClick(object sender, EventArgs e)
		{
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();

			var reportResult = _restrictionNotAbleToBeScheduledReport.Create(_selectedDate,
				_selectedAgents);

			foreach (var restrictionsAbleToBeScheduledResult in reportResult)
			{
				var item = new ListViewItem(restrictionsAbleToBeScheduledResult.Agent.Name.ToString());
				item.SubItems[0].Text = restrictionsAbleToBeScheduledResult.Reason.ToString();
				listViewResult.Items.Add(item);
			}

			listViewResult.ResumeLayout(true);
		}

		public void SetSelected(IEnumerable<IPerson> selectedAgents, DateOnly selectedDate)
		{
			_selectedAgents = selectedAgents;
			_selectedDate = selectedDate;
		}
	}
}
