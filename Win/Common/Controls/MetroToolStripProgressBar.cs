using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WindowsFormsApplication1
{
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
	public class MetroToolStripProgressBar : ToolStripControlHost
	{
		private int _value;

		public MetroToolStripProgressBar() : base(new Label())
		{
			Maximum = 100;
			_value = 0;
			if(DesignMode)
			{
				Size = new Size(20,150);
				BackColor = Color.FromArgb(22,165,220);
				Text = "";
			}
		}

		public int Maximum { get; set; }

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

		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle rec = new Rectangle();
			rec.Width = (int)(e.ClipRectangle.Width * ((double)_value / Maximum));
			rec.Height = e.ClipRectangle.Height - 8;
			e.Graphics.FillRectangle(Brushes.White, rec.Width, 3, e.ClipRectangle.Width, rec.Height);
		}
	}


}
