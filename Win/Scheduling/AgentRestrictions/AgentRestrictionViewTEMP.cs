using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionViewTemp : Form
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IList<IPerson> _persons;
		private readonly RestrictionSchedulingOptions _schedulingOptions;
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private readonly IPerson _selectedPerson;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private AgentRestrictionsDetailView _detailView;

		public AgentRestrictionViewTemp(ISchedulerStateHolder stateHolder, IList<IPerson> persons, RestrictionSchedulingOptions schedulingOptions, IWorkShiftWorkTime workShiftWorkTime, IPerson selectedPerson,
			IGridlockManager lockManager, SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
			IScheduleDayChangeCallback changeCallback, IScheduleTag scheduleTag)
		{
			if(schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			InitializeComponent();
			_stateHolder = stateHolder;
			_persons = persons;
			_schedulingOptions = schedulingOptions;
			_workShiftWorkTime = workShiftWorkTime;
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.UseStudentAvailability = true;
			_schedulingOptions.UseScheduling = true;
			_selectedPerson = selectedPerson;
			agentRestrictionGrid.SelectedAgentIsReady += AgentRestrictionGridSelectedAgentIsReady;
			label1.Text = string.Empty;

			_detailView = new AgentRestrictionsDetailView(grid, _stateHolder, lockManager, schedulePartFilter, clipHandler, overriddenBusinessRulesHolder, changeCallback, scheduleTag, workShiftWorkTime);
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
				IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
				_detailView.LoadDetails(displayRow.Matrix, restrictionExtractor, _schedulingOptions, displayRow.ContractTargetTime);
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
			agentRestrictionGrid.LoadData(_stateHolder, _persons, _schedulingOptions, _workShiftWorkTime, _selectedPerson);
			agentRestrictionGrid.Refresh();	
		}

		private void buttonRecalculate_Click(object sender, EventArgs e)
		{
			_schedulingOptions.UseAvailability = checkBoxAvailability.Checked;
			_schedulingOptions.UseRotations = checkBoxRotation.Checked;
			_schedulingOptions.UsePreferences = checkBoxPreference.Checked;
			_schedulingOptions.UseStudentAvailability = checkBoxStudentAvailability.Checked;
			_schedulingOptions.UseScheduling = checkBoxSchedule.Checked;

			agentRestrictionGrid.LoadData(_schedulingOptions);
		}
	}
}
