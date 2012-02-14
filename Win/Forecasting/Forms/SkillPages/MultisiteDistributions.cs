using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    public partial class MultisiteDistributions : BaseUserControl, IPropertyPage
    {
        public MultisiteDistributions()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            SetColors();

            gridSubSkills.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            gridSubSkills.CutPaste.ClipboardFlags |= GridDragDropFlags.NoAppendRows;
        }

        private void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
            gridSubSkills.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridSubSkills.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            IMultisiteSkill skill = aggregateRoot as IMultisiteSkill;
            LoadChildSkills(skill);
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            IMultisiteSkill skill = aggregateRoot as IMultisiteSkill;
            return GetDistributions(skill);
        }

        private bool GetDistributions(IMultisiteSkill skill)
        {
            decimal percentageSum = 0;
            IDictionary<IChildSkill, Percent> distributions = new Dictionary<IChildSkill, Percent>();
            for (int i = 1; i < gridSubSkills.RowCount; i++)
            {
                GridStyleInfo cell = gridSubSkills[i, 2];
                Guid id = (Guid)cell.Tag;
                IChildSkill child = skill.ChildSkills.FirstOrDefault(c => c.Id.Value == id);

                if (child != null)
                {
                    Percent value = (Percent)cell.CellValue;
                    distributions.Add(child, value);
                    percentageSum += (decimal)value.Value;
                }
            }

            DateTime startDateUtc = skill.TimeZone.ConvertTimeToUtc(SkillDayTemplate.BaseDate, skill.TimeZone);
            DateTimePeriod timePeriod = new DateTimePeriod(
                startDateUtc, startDateUtc.AddDays(1)).MovePeriod(skill.MidnightBreakOffset);

            foreach (IMultisiteDayTemplate template in skill.TemplateMultisiteWeekCollection.Values)
            {
                template.SetMultisitePeriodCollection(
                    new List<ITemplateMultisitePeriod>
                    {
                        new TemplateMultisitePeriod(
                            timePeriod,
                            distributions)
                    });
            }
            if (!(percentageSum == 1m))
            {
                TimedWarningDialog warn =
                    new TimedWarningDialog(3, UserTexts.Resources.SubskillDistributionMustSumToHundredPercent, gridSubSkills);
                warn.ShowDialog(this);
            }

            return (percentageSum == 1m);
        }

        public string PageName
        {
            get { return UserTexts.Resources.SubSkillDistributions; }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }

        #region Grid events
        //private void gridSubSkills_QueryColWidth(object sender, GridRowColSizeEventArgs e)
        //{
        //    if (e.Index == 1)
        //        e.Size = 125;
        //    else if (e.Index == 2)
        //        e.Size = 70;
        //    e.Handled = true;
        //}

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            gridSubSkills.Refresh();
        }

        //private void gridSubSkills_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        //{
        //    e.IgnoreCurrentCell = true;
        //    if (((GridModel)sender).CurrentCellInfo.ColIndex == 2)
        //    {
        //        HandlePaste();
        //    }
        //    e.Handled = true;
        //    gridSubSkills.Refresh();
        //}

        #endregion

        #region Handle Percentages

        #endregion

        #region Copy paste

        //using text in clipboard so we can paste to/from other programs like excel
        //private void HandlePaste()
        //{
        //    ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

        //    if (clipHandler.ClipList.Count > 0)
        //    {
        //        GridRangeInfoList rangelist = GridHelper.GetGridSelectedRanges(gridSubSkills, true);

        //        foreach (GridRangeInfo range in rangelist)
        //        {
        //            //loop all rows in selection, step with height in clip
        //            for (int i = range.Top; i <= range.Bottom; i = i + clipHandler.RowSpan())
        //            {
        //                int row = i;

        //                //loop all columns in selection, step with in clip
        //                for (int j = range.Left; j <= range.Right; j = j + clipHandler.ColSpan())
        //                {
        //                    int col = j;

        //                    if (row > gridSubSkills.Rows.HeaderCount && col > gridSubSkills.Cols.HeaderCount)
        //                    {

        //                        foreach (Clip clip in clipHandler.ClipList)
        //                        {
        //                            //check clip fits inside selected range, rows
        //                            if (GridHelper.IsPasteRangeOk(range, gridSubSkills, clip, i, j))
        //                            {
        //                                Paste(clip, row + clip.RowOffset, col + clip.ColOffset);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void Paste(Clip clip, int rowIndex, int columnIndex)
        //{
        //    if (columnIndex <= int.MinValue)
        //    {
        //        throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
        //    }
        //    else
        //    {
        //        string clipValue = (string)clip.ClipObject;
        //        this.gridSubSkills.Model[rowIndex, columnIndex].ApplyFormattedText(clipValue);
        //    }
        //}

        #endregion

        #region Prepare grid

        private void LoadChildSkills(IMultisiteSkill skill)
        {
            if (!gridSubSkills.CellModels.ContainsKey("MultiSitePercentCell"))
            {
                int childCount = skill.ChildSkills.Count;
                if (childCount == 0) return;
                gridSubSkills.CellModels.Add("MultiSitePercentCell", initializeMultiSitePercentCell());
                gridSubSkills.CellModels.Add("PercentReadOnly", initializePercentReadOnlyCell());
                gridSubSkills.RowCount = childCount + 1;

                double sum = 0d;
                int i = 0;

                gridSubSkills[i, 1].Text = UserTexts.Resources.Name;
                gridSubSkills[i, 2].Text = UserTexts.Resources.Percentage;

                double[] percent = MultisiteHelper.CalculateLowVarianceDistribution(100, childCount, 2);

                for (i = 1; i <= childCount; i++)
                {
                    gridSubSkills[i, 1].CellType = "Static";
                    gridSubSkills[i, 1].Text = skill.ChildSkills[i - 1].Name;
                    gridSubSkills[i, 1].ReadOnly = true;

                    gridSubSkills[i, 2].CellType = "MultiSitePercentCell";
                    gridSubSkills[i, 2].CellValue = new Percent(percent[i - 1]);
                    gridSubSkills[i, 2].Tag = skill.ChildSkills[i - 1].Id.Value;
                    sum += percent[i - 1];
                }
                gridSubSkills[i, 1].CellType = "Static";
                gridSubSkills[i, 1].Text = UserTexts.Resources.Total;
                gridSubSkills[i, 1].ReadOnly = true;

                gridSubSkills[i, 2].CellType = "PercentReadOnly";
                gridSubSkills[i, 2].CellValue = new Percent(sum);
                gridSubSkills[i, 2].HorizontalAlignment = GridHorizontalAlignment.Right;
            }
            gridSubSkills.Model.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
            gridSubSkills.Refresh();
            gridSubSkills.CurrentCellEditingComplete += gridSubSkills_CurrentCellEditingComplete;
        }

        void gridSubSkills_CurrentCellEditingComplete(object sender, EventArgs e)
        {
            gridSubSkills[gridSubSkills.RowCount, 2].CellValue = new Percent(total());
        }

        private double total()
        {
            double percentageSum = 0;
            for (int i = 1; i < gridSubSkills.RowCount; i++)
            {
                GridStyleInfo cell = gridSubSkills[i, 2];
                Percent value = (Percent)cell.CellValue;
                percentageSum += value.Value;
            }
            return percentageSum;
        }

        private void gridSubSkills_CellClick(object sender, GridCellClickEventArgs e)
        {

        }
        //todo this page needs a sum row which is updated by value changed or something...

        private GridCellModelBase initializeMultiSitePercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(gridSubSkills.Model);
            cellModel.NumberOfDecimals = 2;
            cellModel.MinMax = new MinMax<double>(0.00, 1);
            return cellModel;
        }

        private GridCellModelBase initializePercentReadOnlyCell()
        {
            PercentReadOnlyCellModel cellModel = new PercentReadOnlyCellModel(gridSubSkills.Model);
            cellModel.NumberOfDecimals = 2;
            return cellModel;
        }

        #endregion
    }
}
