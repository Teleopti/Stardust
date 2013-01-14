using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class OverviewDrawAbsence
	{
		private readonly Font _font;
		
		public OverviewDrawAbsence(Font font)
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
				drawCellEventArgs.Graphics.FillRectangle(lBrush, rect);
			}

			drawCellEventArgs.Graphics.DrawString(text, _font, Brushes.Black, point);
		}
	}
}
