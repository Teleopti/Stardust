using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsWarningDrawer : IAgentRestrictionsWarningDrawer
	{
		public void Draw(GridDrawCellEventArgs e,  IAgentRestrictionsModel model)
		{
			if(e == null) throw new ArgumentNullException("e");
			if(model == null) throw new ArgumentNullException("model");

			if (e.RowIndex > 1 && e.ColIndex > 0)
			{
				var agentRestrictionsDisplayRow = model.DisplayRowFromRowIndex(e.RowIndex);
				if (agentRestrictionsDisplayRow == null) return;
				var warning = agentRestrictionsDisplayRow.Warning(e.ColIndex);

				if (warning != null)
				{
					var pt1 = new Point(e.Bounds.Right, e.Bounds.Y);
					var pt2 = new Point(e.Bounds.Right - 6, e.Bounds.Y);
					var pt3 = new Point(e.Bounds.Right, e.Bounds.Y + 6);

					e.Graphics.FillPolygon(Brushes.Red, new[] {pt1, pt2, pt3});
				}
				else
				{
					e.Style.CellTipText = string.Empty;
				}
			}
		}
	}
}
