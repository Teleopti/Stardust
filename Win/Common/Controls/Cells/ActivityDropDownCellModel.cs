using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly"), Serializable]
    public class ActivityDropDownCellModel : GridComboBoxCellModel
    {
    	private readonly Image _masterActivityImage;

    	public ActivityDropDownCellModel(GridModel grid, Image masterActivityImage)
            : base(grid)
        {
        	_masterActivityImage = masterActivityImage;
        	AllowFloating = false;
            ButtonBarSize = new Size(0x15, 0);
        }

        protected ActivityDropDownCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new ActivityPickerRenderer(control, this, _masterActivityImage);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if (style == null) return false;
            var activity = style.CellValue as IActivity;
            if (activity != null && activity.Name != text)
            {
                var newActivity = GetActivityFromName(style, text);
                if (newActivity == null)
                    return false;

                style.CellValue = newActivity;
                return true;
            }
            return false;
        }

        private static IActivity GetActivityFromName(GridStyleInfo style, string text)
        {
            return
                ((IEnumerable<IActivity>) style.DataSource).FirstOrDefault(
                    activity =>
                    activity.ToString() == text ||
                    activity.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
        }

        public override bool ApplyText(GridStyleInfo style, string text)
        {
            return ApplyFormattedText(style, text, -1);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            if (style == null) return "";
            if (value == null) return "";
            style.CellValue = value;
            var activity = value as IActivity;
            return activity != null ? activity.Name : ((IActivity) style.CellValue).Name;
        }
    }

    public class ActivityPickerRenderer : GridComboBoxCellRenderer
    {
    	private readonly Image _masterActivityImage;

    	public ActivityPickerRenderer(GridControlBase grid, ActivityDropDownCellModel cellModel, Image masterActivityImage)
            : base(grid, cellModel)
        {
        	_masterActivityImage = masterActivityImage;
        }

    	protected override ListBox CreateListBoxPart()
        {
            var listBox = base.CreateListBoxPart();
            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            listBox.ItemHeight = 18;
            listBox.DrawItem += ListBoxDrawItem;
            return listBox;
        }

        protected override TextBoxBase CreateTextBox()
        {
            var textBox = base.CreateTextBox();
            return textBox;
        }

        private void ListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
			
            var item = ListBoxPart.Items[e.Index];
            var activity = item as IActivity;
            var masterActivity = item as IMasterActivity;
            var color = Color.Black;
            var style = FontStyle.Regular;
            Image theImage = null;
            if (masterActivity != null)
            {
                color = Color.Maroon;
                style = FontStyle.Bold;
                theImage = _masterActivityImage;
            }
            if (activity != null)
            {
                if (theImage != null)
                    e.Graphics.DrawImage(theImage, e.Bounds.Left, e.Bounds.Y, 16, 16);

                var point = new Point(e.Bounds.Left + 20, e.Bounds.Y);
                var size = new Size(e.Bounds.Size.Width - 20, e.Bounds.Size.Height);
                var rect = new Rectangle(point, size);


                using (var fontNew = new Font(e.Font, style))
                using (var customBrush = new SolidBrush(color))
                using (var stringFormat = new StringFormat(StringFormat.GenericDefault))
                {
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                    e.Graphics.DrawString(activity.Name, fontNew, customBrush, rect,
                                          stringFormat);
                }
            }
            e.DrawFocusRectangle();
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex,
                                       GridStyleInfo style)
        {
        	if (g == null) return;
        	if (style == null) return;

        	var color = Color.Black;
        	var fontStyle = FontStyle.Regular;

        	if (style.ImageIndex > -1)
        	{
        		color = Color.Maroon;
        		fontStyle = FontStyle.Bold;
        	}

        	style.Font.Bold = fontStyle == FontStyle.Bold;
        	style.TextColor = color;

        	base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
        }
    }
}
