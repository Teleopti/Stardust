using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class OverviewDrawMainShift
	{
		private readonly Font _font;

		public OverviewDrawMainShift(Font font)
		{
			_font = font;
		}

		public void Draw(GridDrawCellEventArgs drawCellEventArgs, string text, Color color)
		{
			if(drawCellEventArgs == null) throw new ArgumentNullException("drawCellEventArgs");

			var rect = new Rectangle(drawCellEventArgs.Bounds.Location, drawCellEventArgs.Bounds.Size);
			rect.Inflate(-2, -2);

			var stringWidth = drawCellEventArgs.Graphics.MeasureString(text, _font);
			var point = new Point(rect.X - (int)stringWidth.Width / 2 + rect.Width / 2, rect.Y - (int)stringWidth.Height / 2 + rect.Height / 2);

			using (Brush lBrush = new SolidBrush(color))
			{
				GridHelper.FillRoundedRectangle(drawCellEventArgs.Graphics, rect, 2, lBrush, -1);
			}

			drawCellEventArgs.Graphics.DrawString(text, _font, Brushes.Black, point);
		}
	}
}
