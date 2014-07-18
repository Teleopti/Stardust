using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Teleopti.Ccc.Win.Common.Controls
{
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
	public class MetroToolStripProgressBar : ToolStripControlHost
	{
		private int _value;

		public MetroToolStripProgressBar() : base(new Label())
		{
			Maximum = 100;
			_value = 0;
			Step = 1;
			BackColor = Color.FromArgb(22, 165, 220);
			if(DesignMode)
			{
				Size = new Size(20,150);
				BackColor = Color.FromArgb(22,165,220);
				Text = "";
			}
		}

		public int Maximum { get; set; }

		public int Step { get; set; }

		public Label CustomLabelControl
		{
			get { return Control as Label; }
		}

		public int Value
		{
			get { return _value; }
			set
			{
				_value = value;
				Invalidate();
			}
		}

		public void PerformStep()
		{
			if (Value < Maximum)
				Value += Step;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle rec = new Rectangle();
			rec.Width = (int)(e.ClipRectangle.Width * ((double)_value / Maximum));
			e.Graphics.FillRectangle(Brushes.White, rec.Width, 6, e.ClipRectangle.Width, 10);
		}
	}


}
