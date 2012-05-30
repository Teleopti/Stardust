﻿using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsLoadingDrawer : IAgentRestrictionsDrawer
	{
		public bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e, AgentRestrictionsDisplayRow agentRestrictionsDisplayRow)
		{
			if(view == null) throw new ArgumentNullException("view");
			if(e == null) throw new ArgumentNullException("e");
			if(agentRestrictionsDisplayRow == null) throw new ArgumentNullException("agentRestrictionsDisplayRow");

			if (agentRestrictionsDisplayRow.State.Equals(AgentRestrictionDisplayRowState.Loading) && e.ColIndex > (int)AgentRestrictionDisplayRowColumn.DaysOffSchedule)
			{
				e.Style.Text = UserTexts.Resources.NA;
				//view.MergeCells(e.RowIndex, false);
				return true;
			}

			return false;
		}
	}
}
