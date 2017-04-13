using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class OverviewDrawDayOff
	{
		private readonly Color _stripeColor = Color.LightGray;
		
		public void Draw(GridDrawCellEventArgs drawCellEventArgs, Color color)
		{
			if(drawCellEventArgs == null) throw new ArgumentNullException("drawCellEventArgs");

			var rect = new Rectangle(drawCellEventArgs.Bounds.Location, drawCellEventArgs.Bounds.Size);
			rect.Inflate(-2, -2);

			using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, color, _stripeColor))
			{
				drawCellEventArgs.Graphics.FillRectangle(brush, rect);
			}	
		}
	}
}
