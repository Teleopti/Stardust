using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionViewTemp : Form
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IList<IPerson> _persons;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly IRuleSetProjectionService _projectionService;
		private IPerson _selectedPerson;

		public AgentRestrictionViewTemp(ISchedulerStateHolder stateHolder, IList<IPerson> persons, ISchedulingOptions schedulingOptions, IRuleSetProjectionService projectionService, IPerson selectedPerson)
		{
			InitializeComponent();
			_stateHolder = stateHolder;
			_persons = persons;
			_schedulingOptions = schedulingOptions;
			_projectionService = projectionService;
			_selectedPerson = selectedPerson;
			agentRestrictionGrid.SelectedAgentIsReady += AgentRestrictionGridSelectedAgentIsReady;
			label1.Text = string.Empty;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		void AgentRestrictionGridSelectedAgentIsReady(object sender, EventArgs e)
		{
			var eventArgs = e as AgentDisplayRowEventArgs;
			if (eventArgs == null) return;
			var displayRow = eventArgs.AgentRestrictionsDisplayRow;

			if(label1.InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(AgentRestrictionGridSelectedAgentIsReady), sender, e);
			}
			else
			{
				label1.Text = displayRow.AgentName + " is ready";	
			}
			
		}

		private void Button1Click(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.Dispose();
			Close();
		}

		private void AgentRestrictionViewTempLoad(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.MergeHeaders();
			agentRestrictionGrid.LoadData(_stateHolder, _persons, _schedulingOptions, _projectionService, _selectedPerson);
			agentRestrictionGrid.Refresh();	
		}
	}
}
