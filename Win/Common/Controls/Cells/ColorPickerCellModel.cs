using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Diagnostics;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class ColorPickerCellModel:DropDownCellStaticModel
    {
        public ColorPickerCellModel(GridModel grid) : base(grid)
        {
            AllowFloating = false;
            ButtonBarSize = new Size(0x15, 0);
        }

        protected ColorPickerCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected internal Color CellValue { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new ColorPickerRenderer(control, this);
        }

        public static Color ColorFromString(string text)
        {
            object obj = null;
            try
            {
                //following check was added to restrict single numeric values (i.e say 55) from 
                //getting parsed to Color objects. - shirag
                if (text.Contains(";") || text.Contains(","))
                {
                    obj = TypeDescriptor.GetConverter(typeof(Color)).ConvertFrom(text);
                }
                else
                {
                    obj = Color.FromName(text);
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("ColorPickerCellModel: String could not be parsed to Color");
            }
         
            if (obj is Color)
            {
                return (Color)obj;
            }
            return Color.Empty;
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            Color toTest = ColorFromString(text);
            if (toTest != Color.Empty && toTest.ToArgb() != 0)
            {
                style.CellValue = toTest;
                return true;
            }

            return false;
        }

        public override bool ApplyText(GridStyleInfo style, string text)
        {
            return ApplyFormattedText(style, text, -1);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return ColorConvert.ColorToString((Color)style.CellValue, true);
        }
    }

    public class ColorPickerRenderer : StaticDropDownCellRenderer
    {
        private ColorUIControl colorUI;

        public ColorPickerRenderer(GridControlBase grid, ColorPickerCellModel cellModel)
            : base(grid, cellModel)
        {
            DropDownButton = new GridCellButton(this);
            DropDownButton.Text = "...";
            cellModel.CellValue = Color.Red;
        }

        public override void ChildClosing(IPopupChild childUI, PopupCloseType popupCloseType)
        {
            if ((popupCloseType == PopupCloseType.Done) && !IsReadOnly())
            {
                if (!NotifyCurrentCellChanging())
                {
                    return;
                }
                base.ControlValue = colorUI.SelectedColor;

                Grid.InvalidateRange(GridRangeInfo.Cell(RowIndex, ColIndex));
                NotifyCurrentCellChanged();
            }
            DropDownContainerCloseDropDown(childUI, new PopupClosedEventArgs(popupCloseType));
        }

        private void ColorUIColorSelected(object sender, EventArgs e)
        {
            CurrentCell.CloseDropDown(PopupCloseType.Done);
        }

        public override void DropDownContainerShowedDropDown(object sender, EventArgs e)
        {
            DropDownContainer.FocusParent();
            NotifyShowedDropDown();
        }

        public override void DropDownContainerShowingDropDown(object sender, CancelEventArgs e)
        {
            DropDownContainer.Size = new Size(500, 500);
            Color backColor = (Color)base.ControlValue;

            colorUI.Start(backColor);
            Size size = new Size(0xd0, 230);
            GridCurrentCellShowingDropDownEventArgs args = new GridCurrentCellShowingDropDownEventArgs(size);
            Grid.RaiseCurrentCellShowingDropDown(args);
            if (args.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                DropDownContainer.Size = args.Size;
            }
        }

        protected override void InitializeDropDownContainer()
        {
            base.InitializeDropDownContainer();
            colorUI = new ColorUIControl();
            colorUI.Dock = DockStyle.Fill;
            colorUI.Visible = true;
            colorUI.ColorSelected += ColorUIColorSelected;
            DropDownContainer.Controls.Add(colorUI);
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            if (!clientRectangle.IsEmpty)
            {
                int imageIndex = style.ImageIndex;
                if (imageIndex != -1)
                {
                    ImageList imageList = style.ImageList;
                    if ((imageList != null) && (imageIndex < imageList.Images.Count))
                    {
                        Rectangle rect = GetCellBoundsCoreInt(rowIndex, colIndex, true);
                        Rectangle clipBounds = GetCellBoundsCoreInt(rowIndex, colIndex, false);
                        if (!g.ClipBounds.IsEmpty)
                        {
                            clipBounds.Intersect(Rectangle.Ceiling(g.ClipBounds));
                        }
                        if (clipBounds.Contains(rect))
                        {
                            DrawImage(g, imageList, imageIndex, rect, Grid.IsRightToLeft());
                        }
                        else if (clipBounds.IntersectsWith(rect))
                        {
                            DrawImage(g, imageList, imageIndex, rect, clipBounds, Grid.IsRightToLeft());
                        }
                    }
                }
                Rectangle textRectangle = RemoveMargins(clientRectangle, style);
                if (!textRectangle.IsEmpty)
                {
                    bool flag2 = false;
                    if (style.ImageFromByteArray && (style.CellValueType == typeof(byte[])))
                    {
                        try
                        {
                            Image image = ImageUtil.ConvertToImage(style.CellValue);
                            if (image != null)
                            {
                                Rectangle clipRectangle = GetCellBoundsCoreInt(rowIndex, colIndex, false);
                                bool isTextRightToLeft = ((style.RightToLeft == RightToLeft.Inherit) && Grid.IsRightToLeft()) || (style.RightToLeft == RightToLeft.Yes);
                                GridImageUtil.DrawImage(image, clipRectangle, g, clientRectangle, style, isTextRightToLeft);
                                flag2 = true;
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Image conversion from ByteArray failed. Set GridStyleInfo.ImageFromByteArray = false; to prevent this exception.");
                            TraceUtil.TraceExceptionCatched(exception);
                            if (!ExceptionManager.RaiseExceptionCatched(this, exception))
                            {
                                throw;
                            }
                        }
                    }
                    if (!flag2)
                    {
                        GridDrawCellDisplayTextEventArgs e = new GridDrawCellDisplayTextEventArgs(g, string.Empty, textRectangle, style);
                        Grid.RaiseDrawCellDisplayText(e);
                        if (!e.Cancel)
                        {
                            textRectangle = e.TextRectangle;
                            textRectangle.Inflate(-2, -2);
                            if (textRectangle.Width > 0 && textRectangle.Height > 0)
                            {
                                //create a brush rectangle slightly bigger than dest rect
                                Rectangle rect = new Rectangle(textRectangle.X, textRectangle.Y - 1, textRectangle.Width, textRectangle.Height + 2);
                                Color color;
                                if (CurrentCell.RowIndex == e.RowIndex && CurrentCell.ColIndex == e.ColIndex)
                                    color = (Color)base.ControlValue;
                                else
                                    color = (Color)e.Style.CellValue;
                                using (Brush brush = new LinearGradientBrush(rect, color, color, 90, false))
                                {
                                    g.FillRectangle(brush, textRectangle);
                                }
                            } 
                        }
                    }
                }
            }
        }
    }
}