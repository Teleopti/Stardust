using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class SkillIntradayTemplateGridControl : BaseIntradayGridControl
    {
        private readonly RowManager<SkillDataGridRow, ISkillData> _rowManagerTemplateSkillDataPeriod;
        private readonly ISkillDayTemplate _skillDayTemplate;
        private readonly ISkillType _skillType;
        
        public SkillIntradayTemplateGridControl(ISkillDayTemplate skillDayTemplate, TimeZoneInfo timeZone, int resolution, ISkillType skillType) 
            : base(null,null,timeZone,resolution,null, skillType.DisplayTimeSpanAsMinutes, new ChartSettings())
        {
            _skillType = skillType;
            _skillDayTemplate = skillDayTemplate;
            _rowManagerTemplateSkillDataPeriod = new RowManager<SkillDataGridRow, ISkillData>(this, Intervals, Resolution);
            ModifyCells += SkillIntradayTemplateGridControl_ModifyCells;
        }

        private void SkillIntradayTemplateGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
            IList<ITemplateSkillDataPeriod> dataPeriods = e.DataPeriods.OfType<ITemplateSkillDataPeriod>().ToList();
            if (dataPeriods.Count < 1) return;

            switch (e.ModifyCellOption)
            {
                case ModifyCellOption.Merge:
                    if (dataPeriods.Count == 1) return;
                    _skillDayTemplate.MergeTemplateSkillDataPeriods(dataPeriods);
                    break;
                case ModifyCellOption.Split:
                    _skillDayTemplate.SplitTemplateSkillDataPeriods(dataPeriods);
                    break;
            }

            RefreshGrid();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is child skill.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is child skill; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-12
        /// </remarks>
        private bool IsChildSkill
        {
            get { return ((_skillDayTemplate.Parent as ChildSkill) != null); }
        }

    	protected override void CreateGridRows()
        {
            base.CreateGridRows();

            GridRows.Clear();
            GridRows.Add(new IntervalHeaderGridRow(Intervals));
            
            Rows.HeaderCount = 0;

            TextManager manager = new TextManager(_skillType);
			if (_skillType.ForecastSource != ForecastSource.InboundTelephony && _skillType.ForecastSource != ForecastSource.Chat) 
            {
                if (!IsChildSkill)
                {
                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new TemplateSkillServiceLevelGridRow(_rowManagerTemplateSkillDataPeriod, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                        "ServiceLevelTimeSpan", manager.WordDictionary["HandledWithin"])));
                }

                GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "IntegerMinMaxAgentCell",
					"MinimumPersons", manager.WordDictionary["MinimumAgents"])));

                GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "IntegerMinMaxAgentCell",
                    "MaximumPersons", manager.WordDictionary["MaximumAgents"])));

                if (!IsChildSkill)
                {
                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentShrinkageCell",
                        "Shrinkage", UserTexts.Resources.Shrinkage)));

                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentEfficiencyCell",
                        "Efficiency", UserTexts.Resources.Efficiency)));
                }
            }
            else
            {
                if (!IsChildSkill)
                {
                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "ServicePercentCell",
                        "ServiceLevelPercent", UserTexts.Resources.ServiceLevelPercentSign)));

                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "NumericServiceTargetLimitedCell",
                        "ServiceLevelSeconds", UserTexts.Resources.ServiceLevelSeconds)));

                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentCell",
                        "MinOccupancy", UserTexts.Resources.MinimumOccupancy)));

                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentCell",
                        "MaxOccupancy", UserTexts.Resources.MaximumOccupancy)));
                }

                GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "IntegerMinMaxAgentCell",
					"MinimumPersons", manager.WordDictionary["MinimumAgents"])));

                GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "IntegerMinMaxAgentCell",
                    "MaximumPersons", manager.WordDictionary["MaximumAgents"])));

                GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "NullableNumericCell",
					"ManualAgents", manager.WordDictionary["Agents"])));

                if (!IsChildSkill)
                {
                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentShrinkageCell",
                        "Shrinkage", UserTexts.Resources.Shrinkage)));

                    GridRows.Add(_rowManagerTemplateSkillDataPeriod.AddRow(new SkillDataGridRow(_rowManagerTemplateSkillDataPeriod, "PercentEfficiencyCell",
                        "Efficiency", UserTexts.Resources.Efficiency)));
                }
            }
        }

        protected override void MergeSplit(ModifyCellOption options)
        {
            GridRangeInfoList rangelist;

            if (Selections.GetSelectedRanges(out rangelist, true) &&
                rangelist.Count == 1)
            {
                int leftMostCell = rangelist[0].Left;
                int rightMostCell = rangelist[0].Right;
                int row = rangelist[0].Top;

                leftMostCell = (leftMostCell < 1) ? 1 : leftMostCell;
                rightMostCell = (rightMostCell == 0) ? ColCount : rightMostCell;

                if (rangelist[0].IsCols && GridRows.Count > 0)
                    row = GridRows.IndexOf(_rowManagerTemplateSkillDataPeriod.Rows[0]);

                var gridRow = GridRows[row] as SkillDataGridRow;
                if (gridRow == null) return;
                IList<TemplateSkillDataPeriod> taskPeriods = 
                    gridRow.GetMergeData(leftMostCell, rightMostCell).OfType<TemplateSkillDataPeriod>().ToList();
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
            if (_skillDayTemplate == null) return;
            IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>(_skillDayTemplate.TemplateSkillDataPeriodCollection);

            DateTime baseDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, TimeZone);
            if (templateSkillDataPeriods.Count > 0)
            {
                IEnumerable<ITemplateSkillDataPeriod> sortedList = templateSkillDataPeriods.OrderBy(p => p.Period.StartDateTime.Ticks);

                DateTime firstTime = sortedList.First().Period.StartDateTime;
                DateTime lastTime = sortedList.Last().Period.EndDateTime;
                CreateIntervalList(firstTime, lastTime);
            }
            else
            {
                Intervals.Clear();
            }
            _rowManagerTemplateSkillDataPeriod.BaseDate = baseDateUtc;
            _rowManagerTemplateSkillDataPeriod.SetDataSource(templateSkillDataPeriods.OfType<ISkillData>().ToList());
        }
    }
}
