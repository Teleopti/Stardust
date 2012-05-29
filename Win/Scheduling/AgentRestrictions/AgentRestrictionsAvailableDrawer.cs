using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

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
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = agentRestrictionsDisplayRow.ContractTargetTime;
			}

			//Days Off (Schedule Period)
			if (e.ColIndex == 6)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				e.Style.CellValue = agentRestrictionsDisplayRow.TargetDaysOff;
			}

			//Contract time
			if (e.ColIndex == 7)
			{
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = agentRestrictionsDisplayRow.ContractCurrentTime;
			}

			//Days off (Schedule)
			if (e.ColIndex == 8)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				e.Style.CellValue = agentRestrictionsDisplayRow.CurrentDaysOff;
			}

			//Min
			if (e.ColIndex == 9)
			{
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = ((IAgentDisplayData) agentRestrictionsDisplayRow).MinimumPossibleTime;
			}

			//Max
			if (e.ColIndex == 10)
			{
				e.Style.CellType = "TimeSpan";
				e.Style.CellValue = ((IAgentDisplayData) agentRestrictionsDisplayRow).MaximumPossibleTime;
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
				if (agentRestrictionsDisplayRow.Warnings == 0) e.Style.CellValue = UserTexts.Resources.Yes;
				else e.Style.CellValue = UserTexts.Resources.No;
			}


			view.MergeCells(e.RowIndex, true);

			return true;
		}
	}
}
