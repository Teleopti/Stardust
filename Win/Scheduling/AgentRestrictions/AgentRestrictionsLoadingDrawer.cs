using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsLoadingDrawer : IAgentRestrictionsLoadingDrawer
	{
		public bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e)
		{
			if(view == null) throw new ArgumentNullException("view");
			if(e == null) throw new ArgumentNullException("e");

			if(e.RowIndex == 2 && e.ColIndex > 0 && !view.FinishedTest)
			{
				view.MergeCells(e.RowIndex, false);
				e.Style.Text = UserTexts.Resources.LoadingDataTreeDots;
				return true;
			}

			return false;
		}
	}
}
