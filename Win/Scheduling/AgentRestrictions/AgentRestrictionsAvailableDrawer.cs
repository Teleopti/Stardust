using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsAvailableDrawer : IAgentRestrictionsDrawer
	{
		public bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e, IAgentRestrictionsDisplayRow agentRestrictionsDisplayRow)
		{
			if(e == null) throw new ArgumentNullException("e");
			if(agentRestrictionsDisplayRow == null) throw new ArgumentNullException("agentRestrictionsDisplayRow");

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
				//e.Style.CellValue = Model.
			}

			//From
			if (e.ColIndex == 3)
			{
				e.Style.CellType = "Static";
				//e.Style.CellValue = Model.
			}

			//To
			if (e.ColIndex == 4)
			{
				e.Style.CellType = "Static";
				//e.Style.CellValue = Model.
			}

			//Contract Target Time
			if (e.ColIndex == 5)
			{
				e.Style.CellType = "TimeSpan";
				//e.Style.CellValue = Model.
			}

			//Days Off (Schedule Period)
			if (e.ColIndex == 6)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				//e.Style.CellValue = Model.
			}

			//Contract time
			if (e.ColIndex == 7)
			{
				e.Style.CellType = "TimeSpan";
				//e.Style.CellValue = Model.
			}

			//Days off (Schedule)
			if (e.ColIndex == 8)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				//e.Style.CellValue = Model.
			}

			//Min
			if (e.ColIndex == 9)
			{
				e.Style.CellType = "TimeSpan";
				//e.Style.CellValue = Model.
			}

			//Max
			if (e.ColIndex == 10)
			{
				e.Style.CellType = "TimeSpan";
				//e.Style.CellValue = Model.
			}

			//Days Off (Schedule + Restrictions)
			if (e.ColIndex == 11)
			{
				e.Style.CellType = "NumericReadOnlyCellModel";
				//e.Style.CellValue = Model.
			}

			//Ok
			if (e.ColIndex == 12)
			{
				e.Style.CellType = "Static";
				//e.Style.CellValue = Model.
			}

			return true;
		}
	}
}
