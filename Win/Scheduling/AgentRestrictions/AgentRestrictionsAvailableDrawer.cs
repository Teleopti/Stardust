using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsAvailableDrawer : IAgentRestrictionsDrawer
	{
		public bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e, AgentRestrictionsDisplayRow agentRestrictionsDisplayRow)
		{
			if(e == null) throw new ArgumentNullException("e");
			if(agentRestrictionsDisplayRow == null) throw new ArgumentNullException("agentRestrictionsDisplayRow");
			if(view == null) throw new ArgumentNullException("view");

			//Name
			if (e.ColIndex == 0)
			{
				e.Style.CellType = "Header";
				e.Style.CellValue = agentRestrictionsDisplayRow.AgentName;
				e.Style.Tag = agentRestrictionsDisplayRow;
			}

			//Warnings
			if (e.ColIndex == 1)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				e.Style.CellValue = agentRestrictionsDisplayRow.Warnings;
			}

			//Type
			if (e.ColIndex == 2)
			{
				e.Style.CellType = "Static";
				e.Style.CellValue = agentRestrictionsDisplayRow.PeriodType;
			}

			//From
			if (e.ColIndex == 3)
			{
				e.Style.CellType = "Static";
				e.Style.CellValue = agentRestrictionsDisplayRow.StartDate;
			}

			//To
			if (e.ColIndex == 4)
			{
				e.Style.CellType = "Static";
				e.Style.CellValue = agentRestrictionsDisplayRow.EndDate;
			}

			//Contract Target Time
			if (e.ColIndex == 5)
			{
				if (agentRestrictionsDisplayRow.Matrix.SchedulePeriod.Contract.EmploymentType != EmploymentType.HourlyStaff)
				{
					e.Style.CellType = "Static";
					e.Style.CellValue = agentRestrictionsDisplayRow.ContractTargetTimeWithTolerance;
				}
				else
				{
					e.Style.CellType = "Static";
					e.Style.CellValue = agentRestrictionsDisplayRow.ContractTargetTimeHourlyEmployees;
				}
			}

			//Days Off (Schedule Period)
			if (e.ColIndex == 6)
			{
				if (agentRestrictionsDisplayRow.Matrix.SchedulePeriod.Contract.EmploymentType != EmploymentType.HourlyStaff)
				{
					e.Style.CellType = "Static";
					e.Style.CellValue = agentRestrictionsDisplayRow.TargetDaysOffWithTolerance;
					var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
					e.Style.CellTipText = warning ?? string.Empty;
				}
				else
				{
					e.Style.CellType = e.Style.CellType = "Static";
					e.Style.CellValue = agentRestrictionsDisplayRow.TargetDaysOff;
				}
			}

			//Contract time
			if (e.ColIndex == 7)
			{
				e.Style.CellType = "TimeSpan";	
				e.Style.CellValue = agentRestrictionsDisplayRow.ContractCurrentTime;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
				e.Style.CellTipText = warning ?? string.Empty;
			}

			//Days off (Schedule)
			if (e.ColIndex == 8)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				e.Style.CellValue = agentRestrictionsDisplayRow.CurrentDaysOff;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
				e.Style.CellTipText = warning ?? string.Empty;
			}

			//Min
			if (e.ColIndex == 9)
			{
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = ((IAgentDisplayData) agentRestrictionsDisplayRow).MinimumPossibleTime;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
				e.Style.CellTipText = warning ?? string.Empty;
			}

			//Max
			if (e.ColIndex == 10)
			{
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = ((IAgentDisplayData) agentRestrictionsDisplayRow).MaximumPossibleTime;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
				e.Style.CellTipText = warning ?? string.Empty;
			}

			//Days Off (Schedule + Restrictions)
			if (e.ColIndex == 11)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				e.Style.CellValue = ((IAgentDisplayData) agentRestrictionsDisplayRow).ScheduledAndRestrictionDaysOff;
			}

			//Ok
			if (e.ColIndex == 12)
			{
				e.Style.CellType = "Static";
				e.Style.CellValue = agentRestrictionsDisplayRow.Warnings == 0 ? UserTexts.Resources.Yes : UserTexts.Resources.No;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);
				e.Style.CellTipText = warning ?? string.Empty;
			}

			return true;
		}
	}
}
