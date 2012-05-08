using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsWarningDrawer : IAgentRestrictionsWarningDrawer
	{
		public void Draw(GridDrawCellEventArgs e, IAgentRestrictionsDisplayRow agentRestrictionsDisplayRow)
		{
			if(e == null) throw new ArgumentNullException("e");

			var pt1 = new Point(e.Bounds.Right, e.Bounds.Y);
			var pt2 = new Point(e.Bounds.Right - 6, e.Bounds.Y);
			var pt3 = new Point(e.Bounds.Right, e.Bounds.Y + 6);

			e.Graphics.FillPolygon(Brushes.Red, new[] {pt1, pt2, pt3});
		}
	}
}
