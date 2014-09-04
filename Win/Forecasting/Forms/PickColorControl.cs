using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	public partial class PickColorControl : BaseUserControl
	{
		public PickColorControl()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		void dropDownBarItem2BeforePopupItemPaint(object sender, Syncfusion.Windows.Forms.Tools.XPMenus.PopupItemPaintEventArgs drawItemInfo)
		{
			popupControlContainer1.Visible = true;
		}

		public event EventHandler<ColorPickerUIAdv.ColorPickedEventArgs> ColorChanged;
		private void colorPickerUiAdv1Picked(object sender, ColorPickerUIAdv.ColorPickedEventArgs args)
		{
			pictureBox1.BackColor = args.Color;

			// Ensures that the PopupControlContainer is closed after the selection of a color.          
			var cc = sender as ColorPickerUIAdv;
			if (cc == null) return;

			var pcc =
				 cc.Parent as Syncfusion.Windows.Forms.PopupControlContainer;
			if (pcc == null) return;
			pcc.HidePopup(Syncfusion.Windows.Forms.PopupCloseType.Done);
			popupControlContainer1.Size = colorPickerUIAdv1.Size;

			var handler = ColorChanged;
			if (handler != null)
			{
				handler.Invoke(this, args);
			}
		}

		public void SetEnabled(bool enable)
		{
			Enabled = enable;
		}

		public Color ThisColor
		{
			get { return pictureBox1.BackColor; }
			set
			{
				pictureBox1.BackColor = value;
			}
		}
	}
}
