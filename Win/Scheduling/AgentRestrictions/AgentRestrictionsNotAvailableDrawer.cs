using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsNotAvailableDrawer : IAgentRestrictionsNotAvailableDrawer
	{
		public bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e)
		{
			//TEST TEST TEST
			if (view == null) throw new ArgumentNullException("view");
			if (e == null) throw new ArgumentNullException("e");

			if (e.RowIndex == 3 && e.ColIndex > 0 && !view.FinishedTest)
			{
				view.MergeCells(e.RowIndex, false);
				e.Style.Text = UserTexts.Resources.NA;
				return true;
			}

			return false;
		}
	}
}
