using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class ActivityDropDownCellModel : DropDownCellStaticModel
    {
        public ActivityDropDownCellModel(GridModel grid)
            : base(grid)
        {
            AllowFloating = false;
            ButtonBarSize = new Size(0x15, 0);
        }

        protected ActivityDropDownCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new ActivityPickerRenderer(control, this);
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
            return ((IEnumerable<IActivity>)style.DataSource).FirstOrDefault(activity => activity.ToString().Equals(text) || activity.Name.Equals(text));
        }

        public override bool ApplyText(GridStyleInfo style, string text)
        {
            return ApplyFormattedText(style, text, -1);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            if (style == null) return "";
            if(value == null) return "";
            style.CellValue = value;
            var activity = value as IActivity;
            return activity != null ? activity.Name :  ((IActivity)style.CellValue).Name;
        }
    }

    public class ActivityPickerRenderer : StaticDropDownCellRenderer
    {
        private ListBox _listBox;
        private readonly Image _masterImage = Resources.MasterActivity16x16;
        private IList<IActivity> _dataSource = new List<IActivity>();

        public ActivityPickerRenderer(GridControlBase grid, ActivityDropDownCellModel cellModel)
            : base(grid, cellModel)
        {
            DropDownButton = new GridCellComboBoxButton(this);
            MakeListBox();
        }

        private void MakeListBox()
        {
            _listBox = new ListBox();
            _listBox.Dock = DockStyle.Fill;
            _listBox.Visible = true;
            _listBox.DisplayMember = "Name";
            _listBox.DrawMode = DrawMode.OwnerDrawFixed;
            _listBox.ItemHeight = 18;
            
            _listBox.DrawItem += ListBoxDrawItem;
            _listBox.SelectedIndexChanged += ListBoxSelectedIndexChanged;
        }
        public override void ChildClosing(IPopupChild childUI, PopupCloseType popupCloseType)
        {
            if (!NotifyCurrentCellChanging())
            {
                return;
            }
            ControlValue = _listBox.SelectedItem;
            Grid.CurrentCell.Invalidate();
            Grid.InvalidateRange(GridRangeInfo.Cell(RowIndex, ColIndex));
            NotifyCurrentCellChanged();
            Grid.CurrentCell.MoveTo(GridRangeInfo.Cell(RowIndex, ColIndex + 1));
            Grid.CurrentCell.MoveTo(GridRangeInfo.Cell(RowIndex, ColIndex));
            
            DropDownContainerCloseDropDown(childUI, new PopupClosedEventArgs(popupCloseType));
        }

        private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentCell.CloseDropDown(PopupCloseType.Done);
        }

        public override void DropDownContainerShowedDropDown(object sender, EventArgs e)
        {
            DropDownContainer.FocusParent();
            NotifyShowedDropDown();
            var activity = ControlValue as IActivity;
            if (activity != null)
            {
                _listBox.SelectedIndexChanged -= ListBoxSelectedIndexChanged;
                _listBox.SelectedIndex = -1;
                _listBox.SelectedItem = activity;
                _listBox.SelectedIndexChanged += ListBoxSelectedIndexChanged;
            }
        }

        public override void DropDownContainerShowingDropDown(object sender, CancelEventArgs e)
        {
            if (e == null) return;

            var width = Grid.GetColWidth(ColIndex);
            DropDownContainer.Size = new Size(width, 500);

            var cntItems = _dataSource.Count;
            if (cntItems > 15)
                cntItems = 15;

            var size = new Size(width, 18 * cntItems);
            
            var args = new GridCurrentCellShowingDropDownEventArgs(size);
            Grid.RaiseCurrentCellShowingDropDown(args);
            if (args.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                 DropDownContainer.Size = size;
            }
            DropDownContainer.Height = _listBox.Height;
            
        }

        protected override void InitializeDropDownContainer()
        {
            base.InitializeDropDownContainer();
            DropDownContainer.Controls.Add(_listBox);
        }
        
        void ListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var activity = _listBox.Items[e.Index] as IActivity;
            var masterActivity = _listBox.Items[e.Index] as IMasterActivity;
            var color = Color.Black;
            var style = FontStyle.Regular;
            Image theImage = null;
            if (masterActivity != null)
            {
                color = Color.Maroon;
                style = FontStyle.Bold;
                theImage = _masterImage;
            }
            if (activity != null)
            {
                if(theImage != null)
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


        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            if (g == null) return;
            if (style == null) return;

            var activity = style.CellValue as IActivity;

            if (_listBox != null && _listBox.DataSource == null)
            {
                _listBox.SelectedIndexChanged -= ListBoxSelectedIndexChanged;
                _dataSource = (IList<IActivity>)style.DataSource;
                _listBox.DataSource = style.DataSource;
                _listBox.SelectedIndexChanged += ListBoxSelectedIndexChanged;

                _listBox.SelectedItem = activity;
            }

            var masterActivity = activity as IMasterActivity;
            var color = Color.Black;
            var fontStyle = FontStyle.Regular;
            Image theImage = null;

            if (masterActivity != null)
            {
                color = Color.Maroon;
                fontStyle = FontStyle.Bold;
                theImage = _masterImage;
            }

            if (activity == null) return;
            if(theImage != null)
                g.DrawImage(theImage, clientRectangle.X, clientRectangle.Y, 16, 16);

            var point = new Point(clientRectangle.Left + 20, clientRectangle.Y);
            var size = new Size(clientRectangle.Size.Width - 20, clientRectangle.Height);
            var rect = new Rectangle(point, size);

            using (var fontBold = new Font(style.Font.Facename, style.Font.Size, fontStyle))
            {
                using (var customBrush = new SolidBrush(color))
                { g.DrawString(activity.Name, fontBold, customBrush, rect); }
            }
        }

    }
}
