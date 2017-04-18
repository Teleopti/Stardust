using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class MultisiteIntradayTemplateGridControl : BaseIntradayGridControl
    {
        private readonly RowManager<TemplateMultisitePeriodGridRow, ITemplateMultisitePeriod> _rowManagerTemplateMultisitePeriod;
        private readonly IMultisiteDayTemplate _multisiteDayTemplate;
        private readonly IMultisiteSkill _multisiteSkill;

        public MultisiteIntradayTemplateGridControl(IMultisiteSkill multisiteSkill, IMultisiteDayTemplate multisiteDayTemplate, TimeZoneInfo timeZone, int resolution)
            : base(null, null, timeZone, resolution, null, multisiteSkill.SkillType.DisplayTimeSpanAsMinutes, new ChartSettings())
        {
            _multisiteDayTemplate = multisiteDayTemplate;
            _multisiteSkill = multisiteSkill;
            ModifyCells += MultisiteIntradayTemplateGridControl_ModifyCells;
            
            _rowManagerTemplateMultisitePeriod = new RowManager<TemplateMultisitePeriodGridRow, ITemplateMultisitePeriod>(this, Intervals, Resolution);
        }

        protected override void OnCreated()
        {
            base.OnCreated();
            foreach (IChildSkill childSkill in _multisiteSkill.ChildSkills)
            {
                GridRows.Add(_rowManagerTemplateMultisitePeriod.AddRow(new TemplateMultisitePeriodGridRow(_rowManagerTemplateMultisitePeriod, "MultiSitePercentCell",
                    "", childSkill.Name, childSkill)));
            }
            GridRows.Add(new TemplateMultisitePeriodSummaryGridRow(_rowManagerTemplateMultisitePeriod, "MultiSitePercentCell",
                "", UserTexts.Resources.Total));
            RowCount = GridRows.Count - 1;
        }

        private void MultisiteIntradayTemplateGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
            IList<ITemplateMultisitePeriod> dataPeriods = e.DataPeriods.OfType<ITemplateMultisitePeriod>().ToList();
            if (dataPeriods.Count < 1) return;

            switch (e.ModifyCellOption)
            {
                case ModifyCellOption.Merge:
                    if (dataPeriods.Count == 1) return;
                    _multisiteDayTemplate.MergeTemplateMultisitePeriods(dataPeriods);
                    break;
                case ModifyCellOption.Split:
                    _multisiteDayTemplate.SplitTemplateMultisitePeriods(dataPeriods);
                    break;
            }

            RefreshGrid();
        }

    	protected override void CreateGridRows()
        {
            base.CreateGridRows(); //Creates list and add headers
            GridRows.Clear();
            GridRows.Add(new IntervalHeaderGridRow(Intervals));
        }

        protected override void MergeSplit(ModifyCellOption options)
        {
            GridRangeInfoList rangelist;

            if (Selections.GetSelectedRanges(out rangelist, true) &&
                rangelist.Count == 1)
            {
                int leftMostCellAfterHeader = Cols.HeaderCount + 1;

                int leftMostCell = rangelist[0].Left;
                int rightMostCell = rangelist[0].Right;
                int row = rangelist[0].Top;

                leftMostCell = Math.Max(leftMostCell, leftMostCellAfterHeader);
                rightMostCell = (rightMostCell == 0) ? ColCount : rightMostCell;

                if (rangelist[0].IsCols && GridRows.Count > 0)
                    row = GridRows.IndexOf(_rowManagerTemplateMultisitePeriod.Rows[0]);

                TemplateMultisitePeriodGridRow gridRow = GridRows[row] as TemplateMultisitePeriodGridRow;
                if (gridRow == null) return;
            
                IList<ITemplateMultisitePeriod> taskPeriods = gridRow.GetMergeData(leftMostCell, rightMostCell);
                TriggerModifyCells(options, taskPeriods);
            }
        }

		protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
		{
			base.OnShowContextMenu(e);
			ContextMenu.MenuItems[3].Enabled = false;
		}

    	protected override void OnUpdatingDataPeriodList()
        {
            if (_multisiteDayTemplate == null) return;
            IList<ITemplateMultisitePeriod> templateMultisitePeriods = new 
                List<ITemplateMultisitePeriod>(_multisiteDayTemplate.TemplateMultisitePeriodCollection);

            DateTime baseDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, TimeZone);
            if (templateMultisitePeriods.Count > 0)
            {
                IEnumerable<ITemplateMultisitePeriod> sortedList = templateMultisitePeriods.OrderBy(p => p.Period.StartDateTime);

                DateTime firstTime = sortedList.First().Period.StartDateTime;
                DateTime lastTime = sortedList.Last().Period.EndDateTime;
                CreateIntervalList(firstTime, lastTime);
            }
            else
            {
                Intervals.Clear();
            }
            _rowManagerTemplateMultisitePeriod.BaseDate = baseDateUtc;
            _rowManagerTemplateMultisitePeriod.SetDataSource(templateMultisitePeriods);
        }

    }
}