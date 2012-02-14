using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class MultisiteDistributionGrid : GridControl
    {
        //private bool _firstTime = true;
        public MultisiteDistributionGrid()
        {
            setGridProperties();
            SetColors();
        }

        #region Set Grid Properties

        private void setGridProperties()
        {
            SuspendLayout();
            ExcelLikeCurrentCell = true;
            ExcelLikeSelectionFrame = true;
            WantTabKey = true;
            Model.Options.WrapCellBehavior = GridWrapCellBehavior.WrapRow;

            Properties.ColHeaders = true;
            Properties.RowHeaders = false;

            AllowSelection = (((GridSelectionFlags.Cell | GridSelectionFlags.Multiple)
                               | GridSelectionFlags.Shift)
                              | GridSelectionFlags.Keyboard)
                             | GridSelectionFlags.AlphaBlend;
            SmartSizeBox = false;
            ThemesEnabled = true;
            UseRightToLeftCompatibleTextBox = true;
            Font = new Font("Arial", 8.25F);
            Name = "gridSubSkills";
            Properties.MarkColHeader = false;
            Properties.MarkRowHeader = false;
            RowCount = 7;
            ColCount = 1;
            Size = new Size(350, 149);
            TabIndex = 0;
            Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            CutPaste.ClipboardFlags |= GridDragDropFlags.NoAppendRows;
            AccessibilityEnabled = true;
            ActivateCurrentCellBehavior = GridCellActivateAction.DblClickOnCell;

            GridBaseStyle gridBaseStyle2 =
                new GridBaseStyle();
            GridBaseStyle gridBaseStyle3 =
                new GridBaseStyle();
            GridBaseStyle gridBaseStyle4 =
                new GridBaseStyle();
            GridRangeStyle gridRangeStyle1 =
                new GridRangeStyle();

            gridBaseStyle2.Name = "Column Header";
            gridBaseStyle2.StyleInfo.BaseStyle = "Header";
            gridBaseStyle2.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;
            gridBaseStyle3.Name = "Standard";
            gridBaseStyle3.StyleInfo.CellType = "TextBox";
            gridBaseStyle3.StyleInfo.Font.Facename = "Tahoma";
            gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(SystemColors.Window);
            gridBaseStyle4.Name = "Header";
            gridBaseStyle4.StyleInfo.Borders.Bottom = new GridBorder(GridBorderStyle.None);
            gridBaseStyle4.StyleInfo.Borders.Left = new GridBorder(GridBorderStyle.None);
            gridBaseStyle4.StyleInfo.Borders.Right = new GridBorder(GridBorderStyle.None);
            gridBaseStyle4.StyleInfo.Borders.Top = new GridBorder(GridBorderStyle.None);
            gridBaseStyle4.StyleInfo.CellType = "Header";
            gridBaseStyle4.StyleInfo.Font.Bold = true;
            gridBaseStyle4.StyleInfo.Interior =
                new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical,
                                                 Color.FromArgb(((int)(((byte)(203)))),
                                                                               ((int)(((byte)(199)))),
                                                                               ((int)(((byte)(184))))),
                                                 Color.FromArgb(((int)(((byte)(238)))),
                                                                               ((int)(((byte)(234)))),
                                                                               ((int)(((byte)(216))))));
            gridBaseStyle4.StyleInfo.VerticalAlignment = GridVerticalAlignment.Middle;
            BaseStylesMap.AddRange(new GridBaseStyle[]
                                       {
                                           //gridBaseStyle1,
                                           gridBaseStyle2,
                                           gridBaseStyle3,
                                           gridBaseStyle4
                                       });
            ColWidthEntries.AddRange(new GridColWidth[]
                                         {
                                            new GridColWidth(0, 35),
                                            new GridColWidth(0, 35)
                                         });
            DefaultGridBorderStyle = GridBorderStyle.Solid;
            gridRangeStyle1.Range = GridRangeInfo.Table();
            gridRangeStyle1.StyleInfo.Font.Bold = false;
            gridRangeStyle1.StyleInfo.Font.Facename = "Arial";
            gridRangeStyle1.StyleInfo.Font.Italic = false;
            gridRangeStyle1.StyleInfo.Font.Size = 8.25F;
            gridRangeStyle1.StyleInfo.Font.Strikeout = false;
            gridRangeStyle1.StyleInfo.Font.Underline = false;
            gridRangeStyle1.StyleInfo.Font.Unit = GraphicsUnit.Point;
            RangeStyles.AddRange(new GridRangeStyle[]
                                     {
                                         gridRangeStyle1
                                     });
            RowHeightEntries.AddRange(new GridRowHeight[]
                                          {
                                            new GridRowHeight(0, 21),
                                            new GridRowHeight(0, 21)});
            SerializeCellsBehavior = GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            Text = "gridControl1";

            ResumeLayout();
        }

        #endregion

        //protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        //{
        //    base.OnSaveCellInfo(e);
        //    if (e.RowIndex < RowCount && !_firstTime)
        //    {
        //        double total = 0;
        //        for (int i = 1; i < RowCount; i++)
        //        {
        //            total = +((Percent)(this[i, 2].CellValue)).Value;
        //        }
        //        this[RowCount, 2].CellValue = new Percent(total);
        //    }
        //    _firstTime = false;
        //}
        
        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
        }


    }
}