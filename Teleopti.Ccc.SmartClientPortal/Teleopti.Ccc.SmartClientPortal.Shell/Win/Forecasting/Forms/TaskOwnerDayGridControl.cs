using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Tooltip;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class TaskOwnerDayGridControl : TeleoptiGridControl, ITaskOwnerGrid, IAnnotatableGrid
    {
        private TaskOwnerHelper _taskOwnerPeriodHelper;
        private readonly RowManager<ITaskOwnerGridRow, ITaskOwner> _rowManagerTaskOwner;
        private readonly RowManager<MultisiteDayGridRow, IMultisiteDay> _rowManagerMultisiteDay;
        private readonly IList<DateOnly> _dateTimes;

        private AbstractDetailView _owner;
        private IList<IGridRow> _gridRows;
        private IDictionary<DateOnly, IOutlier> _outliers = new Dictionary<DateOnly, IOutlier>();
        private bool _turnOffUpdate;
        private CommentMouseController _commentMouseController;

        private readonly ChartSettings _chartSettings;
        private MultisiteDayGridRow _taskOwnerGridRowMultisiteTemplate;
        private ITaskOwnerGridRow _taskOwnerGridRowSkillTemplate;
        private ITaskOwnerGridRow _taskOwnerGridRowWLTemplate;

        private IList<double> _modifiedItems;
    	private IUnsavedDaysInfo _unsavedDays;
	    private readonly IStatisticHelper _statisticsHelper;


	    public TaskOwnerDayGridControl(IEnumerable<ITaskOwner> taskOwnerDays, TaskOwnerHelper taskOwnerPeriodHelper, AbstractDetailView owner, ChartSettings chartSettings, IStatisticHelper statisticsHelper)
            : this(taskOwnerDays, null, taskOwnerPeriodHelper, owner, chartSettings, statisticsHelper)
        {
        }

        public TaskOwnerDayGridControl(IEnumerable<ITaskOwner> taskOwnerDays, IEnumerable<IMultisiteDay> multisiteDays, TaskOwnerHelper taskOwnerPeriodHelper, AbstractDetailView owner, ChartSettings chartSettings, IStatisticHelper statisticsHelper)
        {
            _owner = owner;
            _taskOwnerPeriodHelper = taskOwnerPeriodHelper;
            _dateTimes = taskOwnerDays.Select(t => t.CurrentDate).ToList();
            _rowManagerTaskOwner = new RowManager<ITaskOwnerGridRow, ITaskOwner>(this, null, -1);
            _rowManagerTaskOwner.SetDataSource(taskOwnerDays);
            _rowManagerMultisiteDay = new RowManager<MultisiteDayGridRow, IMultisiteDay>(this, null, -1);
            _rowManagerMultisiteDay.SetDataSource(multisiteDays);
            _chartSettings = chartSettings;
	        _statisticsHelper = statisticsHelper;
	        TeleoptiStyling = true;
        }

        public void Create()
        {
            if (_owner != null && _owner.TargetType == TemplateTarget.Workload)
            {
                _commentMouseController = new CommentMouseController(this);
                _commentMouseController.ContextMenuEnabled = true;
                MouseControllerDispatcher.Add(_commentMouseController);
            }
            createGridRows();
            initializeGrid();
            CreateContextMenu();
        }

        public void CreateContextMenu()
        {
            var gridItemModify = new MenuItem(UserTexts.Resources.ModifySelection, ModifySelectionOnClick);
            var gridItemSave = new MenuItem(UserTexts.Resources.SaveAsTemplate, SaveAsTemplateOnClick);
            var menu = new ContextMenu();
            menu.MenuItems.Add(gridItemModify);
            menu.MenuItems.Add(gridItemSave);
            gridItemModify.Enabled = true;
            gridItemSave.Enabled = true;
            ContextMenu = menu;
        }

        private void ModifySelectionOnClick(object sender, EventArgs e)
        {
            var modifySelectedList = _modifiedItems;
            var numbers = new ModifyCalculator(modifySelectedList);
            var modifySelection = new ModifySelectionView(numbers);
            if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
            var receivedValues = modifySelection.ModifiedList;
            GridHelper.ModifySelectionInput(this, receivedValues);
        }

        private void SaveAsTemplateOnClick(object sender, EventArgs e)
        {
			if (_taskOwnerGridRowMultisiteTemplate != null)
			{
				var templateDays = GetSelectedTemplateDays(TemplateTarget.Multisite);
				if (templateDays.Count() != 1) return;
				var multisiteDay = templateDays[0] as IMultisiteDay;
				SaveAsMultisiteTemplate(multisiteDay);
			}
			else if (_taskOwnerGridRowSkillTemplate != null)
			{
				var templateDays = GetSelectedTemplateDays(TemplateTarget.Skill);
				if (templateDays.Count() != 1) return;
				var skillDay = templateDays[0] as ISkillDay;
				SaveAsSkillTemplate(skillDay);
			}
			else if (_taskOwnerGridRowWLTemplate != null)
			{
				var templateDays = GetSelectedTemplateDays(TemplateTarget.Workload);
				if (templateDays.Count() != 1) return;
				var workloadDay = templateDays[0] as IWorkloadDay;
				SaveAsWorkloadTemplate(workloadDay);
			}
        }

    	private void SaveAsMultisiteTemplate(IMultisiteDay multisiteDay)
    	{
    		if (multisiteDay == null) throw new ArgumentNullException("multisiteDay");
			using(var editMultisiteDayTemplate = new EditMultisiteDayTemplate(multisiteDay))
				editMultisiteDayTemplate.ShowDialog(this);
    	}

    	private void SaveAsSkillTemplate(ISkillDay skillDay)
    	{
    		if (skillDay == null) throw new ArgumentNullException("skillDay");
			using(var editSkillDayTemplate = new EditSkillDayTemplate(skillDay))
    			editSkillDayTemplate.ShowDialog(this);
    	}

    	private void SaveAsWorkloadTemplate(IWorkloadDay workloadDay)
    	{
    		if (workloadDay == null) throw new ArgumentNullException("workloadDay");
    		var openHours = workloadDay.OpenHourList;
			using(var editWorkloadDayTemplate = new EditWorkloadDayTemplate(workloadDay, openHours,_statisticsHelper))
				editWorkloadDayTemplate.ShowDialog(this);
    	}

    	protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            bool enableMenu;
            bool enableMenuSave;
            _modifiedItems = new List<double>();
            _modifiedItems.Clear();

			GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);

			if (_taskOwnerGridRowMultisiteTemplate != null)
				GridHelper.SaveAsTemplateEnabled(this, _taskOwnerGridRowMultisiteTemplate, out enableMenuSave);
			else if (_taskOwnerGridRowSkillTemplate != null)
				GridHelper.SaveAsTemplateEnabled(this, _taskOwnerGridRowSkillTemplate, out enableMenuSave);
			else if (_taskOwnerGridRowWLTemplate != null)
				GridHelper.SaveAsTemplateEnabled(this, _taskOwnerGridRowWLTemplate, out enableMenuSave);
			else enableMenuSave = false;

            ContextMenu.MenuItems[0].Enabled = enableMenu;
            ContextMenu.MenuItems[1].Enabled = enableMenuSave;
            base.OnShowContextMenu(e);
        }

        #region Create grid rows

        private IChartSeriesSetting configureSetting(string key)
        {
            IChartSeriesSetting ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        private void createGridRows()
        {
            _gridRows = new List<IGridRow>();
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekDates, _dateTimes));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthDayNumber, _dateTimes));

            if (_owner.TargetType == TemplateTarget.Workload)
            {
                if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && _owner.SkillType.ForecastSource != ForecastSource.Chat)
                    CreateWorkloadNonTelephonyRows();
                else
                    CreateWorkloadTelephonyRows();
            }
            else if (_owner.TargetType == TemplateTarget.Skill)
            {
                if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && _owner.SkillType.ForecastSource != ForecastSource.Chat)
                    CreateSkillNonTelephonyRows();
                else
                    CreateSkillTelephonyRows();

                if (_rowManagerMultisiteDay.DataSource.Count > 0)
                {
                    _taskOwnerGridRowMultisiteTemplate = new MultisiteDayGridRow(_rowManagerMultisiteDay, "",
                    "TemplateReference.TemplateName", UserTexts.Resources.MultisiteTemplate, _dateTimes);
                    _taskOwnerGridRowMultisiteTemplate.QueryCellValue += taskOwnerGridRowMultisiteTemplate_QueryCellValue;
                    _taskOwnerGridRowMultisiteTemplate.SaveCellValue += taskOwnerGridRowMultisiteTemplate_SaveCellValue;
                    _taskOwnerGridRowMultisiteTemplate.TemplateSelected += taskOwnerGridRowMultisiteTemplate_TemplateSelected;
                    _gridRows.Add(_rowManagerMultisiteDay.AddRow(_taskOwnerGridRowMultisiteTemplate));
                }
            }
			_owner.ChangeUnsavedDaysStyle +=_owner_ChangeUnsavedDaysStyle;
        }

    	private void _owner_ChangeUnsavedDaysStyle(object sender, CustomEventArgs<IUnsavedDaysInfo> e)
    	{
    		_unsavedDays = e.Value;
    	}

    	private void CreateSkillTelephonyRows()
        {
			var manager = GetManager(_owner);
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
				"TotalTasks", manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell",
				"ForecastedIncomingDemand", manager.WordDictionary["ForecastedIncomingDemand"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell",
				"ForecastedIncomingDemandWithShrinkage", manager.WordDictionary["ForecastedIncomingDemandWithShrinkage"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            _taskOwnerGridRowSkillTemplate = new ITaskOwnerGridRow(_rowManagerTaskOwner, "",
                                                                                    "TemplateReference.TemplateName", UserTexts.Resources.Template, UserTexts.Resources.Forecasted, _dateTimes, TemplateTarget.Skill);
            _taskOwnerGridRowSkillTemplate.QueryCellValue += taskOwnerGridRowSkillTemplate_AfterQueryCellInfo;
            _taskOwnerGridRowSkillTemplate.SaveCellValue += taskOwnerGridRowSkillTemplate_AfterSaveCellInfo;
            _taskOwnerGridRowSkillTemplate.TemplateSelected += taskOwnerGridRowSkillTemplate_TemplateSelected;
            _gridRows.Add(_rowManagerTaskOwner.AddRow(_taskOwnerGridRowSkillTemplate));
        }

        private void CreateSkillNonTelephonyRows()
        {

            var manager = GetManager(_owner);

            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalTasks", manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"ForecastedIncomingDemand", manager.WordDictionary["ForecastedIncomingDemand"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"ForecastedIncomingDemandWithShrinkage", manager.WordDictionary["ForecastedIncomingDemandWithShrinkage"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            _taskOwnerGridRowSkillTemplate = new ITaskOwnerGridRow(_rowManagerTaskOwner, "",
                                                                                    "TemplateReference.TemplateName", UserTexts.Resources.Template, UserTexts.Resources.Forecasted, _dateTimes, TemplateTarget.Skill);
            _taskOwnerGridRowSkillTemplate.QueryCellValue += taskOwnerGridRowSkillTemplate_AfterQueryCellInfo;
            _taskOwnerGridRowSkillTemplate.SaveCellValue += taskOwnerGridRowSkillTemplate_AfterSaveCellInfo;
            _taskOwnerGridRowSkillTemplate.TemplateSelected += taskOwnerGridRowSkillTemplate_TemplateSelected;
            _gridRows.Add(_rowManagerTaskOwner.AddRow(_taskOwnerGridRowSkillTemplate));
        }

        private void CreateWorkloadTelephonyRows()
        {
			var manager = new TextManager(_owner.SkillType);
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner,
                                                              "NumericWorkloadDayTaskLimitedCell", "Tasks",
															  manager.WordDictionary["Tasks"],
                                                              UserTexts.Resources.Forecasted, _dateTimes, 12);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTasks",
											manager.WordDictionary["CampaignTasks"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
											"AverageTaskTime", manager.WordDictionary["AverageTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTaskTime",
											manager.WordDictionary["CampaignTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
											"AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell",
											"CampaignAfterTaskTime", manager.WordDictionary["CampaignAfterTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks",
											manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted,
                                            _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"],
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            _taskOwnerGridRowWLTemplate = new ITaskOwnerGridRow(_rowManagerTaskOwner, "",
                                                                                 "TemplateReference.TemplateName",
                                                                                 UserTexts.Resources.Template,
                                                                                 UserTexts.Resources.Forecasted,
                                                                                 _dateTimes);
            _taskOwnerGridRowWLTemplate.QueryCellValue += taskOwnerGridRowWLTemplate_AfterQueryCellInfo;
            _taskOwnerGridRowWLTemplate.SaveCellValue += taskOwnerGridRowWLTemplate_AfterSaveCellInfo;
            _taskOwnerGridRowWLTemplate.TemplateSelected += taskOwnerGridRowWLTemplate_TemplateSelected;
            _gridRows.Add(_rowManagerTaskOwner.AddRow(_taskOwnerGridRowWLTemplate));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"],
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"],
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"],
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalStatisticAverageTaskTime", manager.WordDictionary["TotalStatisticAverageTaskTime"],
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalStatisticAverageAfterTaskTime", manager.WordDictionary["TotalStatisticAverageAfterTaskTime"],
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateWorkloadNonTelephonyRows()
        {
            var manager = new TextManager(_owner.SkillType);

            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericWorkloadDayTaskLimitedCell", 
				"Tasks", manager.WordDictionary["Tasks"], UserTexts.Resources.Forecasted, _dateTimes, 12);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignTasks", manager.WordDictionary["CampaignTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", 
				"AverageTaskTime", manager.WordDictionary["AverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignTaskTime", manager.WordDictionary["CampaignTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
				"AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignAfterTaskTime", manager.WordDictionary["CampaignAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalTasks", manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            _taskOwnerGridRowWLTemplate = new ITaskOwnerGridRow(_rowManagerTaskOwner, "",
                                                                                 "TemplateReference.TemplateName", UserTexts.Resources.Template, UserTexts.Resources.Forecasted, _dateTimes);
            _taskOwnerGridRowWLTemplate.QueryCellValue += taskOwnerGridRowWLTemplate_AfterQueryCellInfo;
            _taskOwnerGridRowWLTemplate.SaveCellValue += taskOwnerGridRowWLTemplate_AfterSaveCellInfo;
            _taskOwnerGridRowWLTemplate.TemplateSelected += taskOwnerGridRowWLTemplate_TemplateSelected;
            _gridRows.Add(_rowManagerTaskOwner.AddRow(_taskOwnerGridRowWLTemplate));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel",
				"TotalStatisticAverageTaskTime", manager.WordDictionary["TotalStatisticAverageTaskTime"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalStatisticAverageAfterTaskTime", manager.WordDictionary["TotalStatisticAverageAfterTaskTime"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        #endregion

        public event EventHandler<TemplateEventArgs> TemplateSelected;

        private void TriggerTemplateSelected(TemplateEventArgs eventArgs)
        {
        	var handler = TemplateSelected;
            if (handler != null)
            {
                handler.Invoke(this, eventArgs);
            }
        }

        private void taskOwnerGridRowWLTemplate_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void taskOwnerGridRowSkillTemplate_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void taskOwnerGridRowMultisiteTemplate_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void taskOwnerGridRowMultisiteTemplate_SaveCellValue(object sender, FromCellEventArgs<IMultisiteDay> e)
        {
            string templateName = e.Value.ToString();

            IMultisiteDay realMultisiteDay = e.Item;
            if (realMultisiteDay != null)
            {
                IMultisiteDayTemplate multisiteDayTemplate =
                    realMultisiteDay.Skill.TryFindTemplateByName(TemplateTarget.Multisite, templateName) as IMultisiteDayTemplate;
                if (multisiteDayTemplate != null)
                {
                    realMultisiteDay.ApplyTemplate(multisiteDayTemplate);
                    realMultisiteDay.RedistributeChilds();
                    RefreshAppliedTemplate(e.Style);
                }
            }
        }

        private void taskOwnerGridRowMultisiteTemplate_QueryCellValue(object sender, FromCellEventArgs<IMultisiteDay> e)
        {
            TaskOwnerDayGridHelper.SetTemplateCellStyle(e.Style, e.Item, e.Item.MultisiteDayDate.DayOfWeek);
        }

        private void taskOwnerGridRowSkillTemplate_AfterSaveCellInfo(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            string templateName = e.Value.ToString();

            ISkillDay realSkillDay = e.Item as ISkillDay;
            if (realSkillDay != null)
            {
                ISkillDayTemplate skillDayTemplate =
                    realSkillDay.Skill.TryFindTemplateByName(TemplateTarget.Skill, templateName) as ISkillDayTemplate;
                if (skillDayTemplate != null)
                {
                    realSkillDay.ApplyTemplate(skillDayTemplate);
                    RefreshAppliedTemplate(e.Style);
                }
            }
        }

        private void RefreshAppliedTemplate(GridStyleInfo gridStyle)
        {
            if (!_turnOffUpdate)
            {
                RefreshRange(GridRangeInfo.Col(gridStyle.CellIdentity.ColIndex));
            }
            else
            {
                RefreshRange(GridRangeInfo.Cell(gridStyle.CellIdentity.RowIndex, gridStyle.CellIdentity.ColIndex));
            }
        }

        private void taskOwnerGridRowSkillTemplate_AfterQueryCellInfo(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            TaskOwnerDayGridHelper.SetTemplateCellStyle(e.Style, e.Item as ITemplateDay, e.Item.CurrentDate.DayOfWeek);
        }

        private void taskOwnerGridRowWLTemplate_AfterSaveCellInfo(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            string templateName = e.Value.ToString();

            IWorkloadDay realWorkloadDay = e.Item as IWorkloadDay;
            if (realWorkloadDay != null)
            {
                IWorkloadDayTemplate workloadDayTemplate =
                    realWorkloadDay.Workload.TryFindTemplateByName(TemplateTarget.Workload, templateName) as IWorkloadDayTemplate;
                if (workloadDayTemplate != null)
                {
                    if (!workloadDayTemplate.OpenForWork.IsOpen && realWorkloadDay.OpenForWork.IsOpen)
                        TaskOwnerDayGridHelper.ResetClosedDay(realWorkloadDay);
                    if (!_turnOffUpdate) realWorkloadDay.Lock();
					realWorkloadDay.ApplyTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
                    if (!_turnOffUpdate) realWorkloadDay.Release();
                    RefreshAppliedTemplate(e.Style);
                }
            }
        }

        private void taskOwnerGridRowWLTemplate_AfterQueryCellInfo(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            TaskOwnerDayGridHelper.SetTemplateCellStyle(e.Style, e.Item as ITemplateDay, e.Item.CurrentDate.DayOfWeek);
        }

        public void SetTemplate(string templateName, TemplateTarget templateTarget)
        {
            if (Selections.Count < 1 || _gridRows.Count == 0) return;

            IList<ITemplateDay> templateDay = GetSelectedTemplateDays(templateTarget);
            BeginGridUpdate();
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.BeginUpdate();
            switch (templateTarget)
            {
                case TemplateTarget.Workload:
                    IWorkload workload = ((WorkloadDetailView)_owner).Workload;
                    workload.SetTemplatesByName(templateTarget, templateName, templateDay);
                    break;
                case TemplateTarget.Skill:
                case TemplateTarget.Multisite:
                    ISkill skill = (ISkill)_owner.GetType().GetProperty("Skill")
                      .GetValue(_owner, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
                    skill.SetTemplatesByName(templateTarget, templateName, templateDay);
                    break;
            }
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.EndUpdate();
            EndGridUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
            RefreshVisibleRange();
        }

        public void ResetTemplates(TemplateTarget templateTarget)
        {
            if (Selections.Count < 1 || _gridRows.Count == 0) return;

            IList<ITemplateDay> templateDay = GetSelectedTemplateDays(templateTarget);
            BeginGridUpdate();
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.BeginUpdate();
            switch (templateTarget)
            {
                case TemplateTarget.Workload:
                    ((WorkloadDetailView)_owner).Workload.SetDefaultTemplates(templateDay);
                    break;
                case TemplateTarget.Skill:
                case TemplateTarget.Multisite:
                    ISkill skill = (ISkill)_owner.GetType().GetProperty("Skill")
                        .GetValue(_owner, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
                    skill.SetDefaultTemplates(templateDay);
                    break;
            }
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.EndUpdate();
            EndGridUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();

            RefreshVisibleRange();
        }

        private void RefreshVisibleRange()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshVisibleRange));
                return;
            }
            RefreshRange(ViewLayout.VisibleCellsRange);
        }

        public void ResetLongterm()
        {
            if (Selections.Count < 1 || _gridRows.Count == 0) return;

            IList<ITemplateDay> templateDay = GetSelectedTemplateDays(TemplateTarget.Workload);
            BeginGridUpdate();
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.BeginUpdate();
            ((WorkloadDetailView)_owner).Workload.SetLongtermTemplate(templateDay);
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.EndUpdate();
            EndGridUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
            RefreshVisibleRange();
        }

        public void BeginGridUpdate()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(BeginGridUpdate));
                return;
            }
            BeginUpdate();
        }

        public void EndGridUpdate()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(EndGridUpdate));
                return;
            }
            EndUpdate();
        }
        
        private IList<ITemplateDay> GetSelectedTemplateDays(TemplateTarget templateTarget)
        {
            IList<ITemplateDay> templateDay = new List<ITemplateDay>();
            foreach (GridRangeInfo range in Selections.Ranges)
            {
                GridRangeInfo rangeInfo = range.IntersectRange(GridRangeInfo.Cols(ColHeaderCount, ColCount));
                if (rangeInfo.IsEmpty) continue;
                for (int i = rangeInfo.Left; i <= rangeInfo.Right; i++)
                {
                    int dataSourceIndex = i - ColHeaderCount;
                    if (dataSourceIndex < 0) continue;
                    if (templateTarget != TemplateTarget.Multisite)
                    {
                        templateDay.Add((ITemplateDay)_rowManagerTaskOwner.DataSource[dataSourceIndex]);
                    }
                    else if (_rowManagerMultisiteDay.DataSource.Count > 0)
                        templateDay.Add(_rowManagerMultisiteDay.DataSource[dataSourceIndex]);
                }
            }
            return templateDay;
        }

        #region InitializeGrid

        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow | GridMergeCellsMode.MergeRowsInColumn;
            Cols.Size[1] = ColorHelper.GridHeaderColumnWidth();// 100;
            DefaultColWidth = 50;
            RowCount = _gridRows.Count - 1;
            ColCount = _dateTimes.Count + ColHeaderCount;
            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;
            Cols.HeaderCount = 1;
            Rows.HeaderCount = 1;
            Rows.SetFrozenCount(1, false);
            Cols.SetFrozenCount(1, false);
            Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            Refresh();
            //new ExcelLikeOutlineHeader(this); ==> Use this when Office2007 styles are compatible with marked headers
        }

        protected override void OnAfterPaste()
        {
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.EndUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
            _turnOffUpdate = false;
        }

        protected override void OnBeforePaste()
        {
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.BeginUpdate();
            _turnOffUpdate = true;
        }

        #endregion

        #region DrawValuesInGrid

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            base.OnQueryCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0 || e.RowIndex >= _gridRows.Count) return;
            if (e.ColIndex > 1 && e.ColIndex - 1 <= _rowManagerTaskOwner.DataSource.Count)
            {
                formatCell(e.Style, _rowManagerTaskOwner.DataSource[e.ColIndex - ColHeaderCount]);
            }
            if (e.Style.CellIdentity == null)
                return;
            _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

            if (e.ColIndex == 1 && e.RowIndex == 0)
            {
                StringBuilder infoText = new StringBuilder();
                if (_owner.TargetType != TemplateTarget.Workload)
                {
                    infoText.Append(UserTexts.Resources.Skill);
                }
                else
                {
                    infoText.Append(UserTexts.Resources.Workload);
                }
                infoText.Append(string.Concat(" - ", UserTexts.Resources.Day));

                e.Style.CellValue = infoText.ToString();
			}
            e.Handled = true;
        }

        private void formatCell(GridStyleInfo gridStyleInfo, ITaskOwner taskOwner)
        {
            IWorkloadDay workloadDay = taskOwner as IWorkloadDay;
            if (gridStyleInfo.BaseStyle == "Header")
            {
                weekendOrWeekday(gridStyleInfo, taskOwner.CurrentDate, false, true);
				
                if (workloadDay != null &&
                    IsAnnotatableCell(gridStyleInfo.CellIdentity.ColIndex, gridStyleInfo.CellIdentity.RowIndex))
                {
                    IAnnotatable annotatable = (IAnnotatable)taskOwner;
                    
					var gridExcelTipStyleProperties = new GridExcelTipStyleProperties(gridStyleInfo);
					gridExcelTipStyleProperties.ExcelTipText = annotatable.Annotation;
                }
            }
            else if (!taskOwner.OpenForWork.IsOpen)
            {
                gridStyleInfo.TextColor = ColorFontClosedCell;
                gridStyleInfo.Font.FontStyle = FontClosedCell;
                gridStyleInfo.BackColor = ColorEditableCell;
                weekendOrWeekday(gridStyleInfo, taskOwner.CurrentDate, true, false);
                bool readOnly = true;
                if (workloadDay != null &&
                    workloadDay.Workload.Skill.SkillType.ForecastSource != ForecastSource.InboundTelephony &&
					workloadDay.Workload.Skill.SkillType.ForecastSource != ForecastSource.Retail &&
					workloadDay.Workload.Skill.SkillType.ForecastSource != ForecastSource.Chat)
                {
                    readOnly = false;
                }
                gridStyleInfo.ReadOnly = readOnly;
            }
            else
            {
                gridStyleInfo.Enabled = true;
                gridStyleInfo.ReadOnly = false;
                gridStyleInfo.BackColor = ColorEditableCell;
                weekendOrWeekday(gridStyleInfo, taskOwner.CurrentDate, true, false);
			}
			changeUnsavedDaysStyle(gridStyleInfo, taskOwner.CurrentDate);
        }

    	private void changeUnsavedDaysStyle(GridStyleInfo gridStyleInfo, DateOnly localCurrentDate)
    	{
			if (_unsavedDays == null) return;
    		if(_unsavedDays.ContainsDateTime(localCurrentDate))
    		{
    			gridStyleInfo.BackColor = ColorHelper.UnsavedDayColor;
    		}
    	}

    	#endregion

        #region SetValuesInGrid

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            if (!_turnOffUpdate) _taskOwnerPeriodHelper.BeginUpdate();

            _gridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

            if (!_turnOffUpdate)
            {
                _taskOwnerPeriodHelper.EndUpdate();
                if (_owner != null) _owner.TriggerValuesChanged();
                RefreshRange(GridRangeInfo.Col(e.ColIndex));
            }

            e.Handled = true;
        }

        #endregion

        #region HandleOpenHours

        //private void AddOpenHour(TimePeriod period, GridStyleInfo gridStyleInfo)
        //{
        //    WorkloadDay workloadDay = _rowManagerTaskOwner.DataSource[gridStyleInfo.CellIdentity.ColIndex-1] as WorkloadDay;
        //    if (workloadDay != null)
        //    {
        //        IList<TimePeriod> openHours = new List<TimePeriod>();
        //        //Multiple openHours should not be implemented yet
        //        //foreach (TimePeriod tp in ((WorkloadDay)this[row, col].Tag).OpenHourList)
        //        //{
        //        //    openHours.Add(tp);
        //        //}
        //        openHours.Add(period);
        //        _taskOwnerPeriodHelper.BeginUpdate();
        //        workloadDay.ChangeOpenHours(openHours);
        //        _taskOwnerPeriodHelper.EndUpdate();
        //        gridStyleInfo.CellValue = openHours;
        //    }
        //}



        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (CurrentCell.ColIndex < 0) return;
            if (CurrentCell.RowIndex == 0 || CurrentCell.RowIndex == 1)
            {
                int rowHeaders = Cols.HeaderCount + 1;

                WorkloadDayBase day = null;
                if (CurrentCell.ColIndex > Cols.HeaderCount)
                    day = _rowManagerTaskOwner.DataSource[CurrentCell.ColIndex - rowHeaders] as WorkloadDayBase;
                if (e.KeyCode == Keys.Delete && day != null)
                    day.ResetTaskOwner();
            }
            base.OnKeyDown(e);
        }

        protected override void OnResizingColumns(GridResizingColumnsEventArgs e)
        {
            base.OnResizingColumns(e);
            if (e.Reason == GridResizeCellsReason.DoubleClick)
            {
                ColWidths.ResizeToFit(Selections.Ranges[0], GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
                e.Cancel = true;
            }
        }

        #endregion

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.Range.Left >= ColHeaderCount)
            {
                ITaskOwner taskOwner = _rowManagerTaskOwner.DataSource[e.Range.Left - ColHeaderCount];

                _turnOffUpdate = true;
                _owner.CurrentDay = taskOwner.CurrentDate;
                _turnOffUpdate = false;

                foreach (IGridRow gridRow in _gridRows)
                {
                    gridRow.OnSelectionChanged(e, ColHeaderCount);
                }
            }

            if (e.Range.Top > 1)
            {
                GridRow gridRow = _gridRows[e.Range.Top] as GridRow;
                if (gridRow != null) _owner.TriggerCellClicked(this, gridRow);
            }
        }

        public void SetOutliers(IDictionary<DateOnly, IOutlier> outliers)
        {
            _outliers = outliers;
        }

        public bool HasColumns
        {
            get { return ColCount > 0; }
        }

        public void RefreshGrid()
        {
            Refresh();
            RefreshVisibleRange();
        }

        /// <summary>
        /// Goes to date.
        /// and select the cell 
        /// (same row if it is known)
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void GoToDate(DateOnly theDate)
        {
            if (_turnOffUpdate) return;
            ITaskOwner taskOwner = _rowManagerTaskOwner.DataSource.FirstOrDefault(wd => wd.CurrentDate == theDate);

            if (taskOwner != null)
            {
                int columnIndex = _rowManagerTaskOwner.DataSource.IndexOf(taskOwner);
                int rowIndex = GetCurrentRowIndex();

                Model.ScrollCellInView(GridRangeInfo.Col(columnIndex + ColHeaderCount), GridScrollCurrentCellReason.MoveTo);
                Model.Selections.Clear();
                Model.Selections.SelectRange(GridRangeInfo.Cell(rowIndex, columnIndex + ColHeaderCount), true);
            }
        }

        private int GetCurrentRowIndex()
        {
            if (Selections.Count == 0) return 0;

            return Selections.Ranges[0].Top;
        }

        public DateOnly GetLocalCurrentDate(int column)
        {
            if (column > int.MaxValue) throw new ArgumentOutOfRangeException("column");

            int count = _rowManagerTaskOwner.DataSource.Count;
            if (count == 0)
                return DateOnly.MaxValue;
            column = Math.Min(column, count); //Get count if column is larger than actual number of items
            column = Math.Max(column, ColHeaderCount); //Get 1 if column is less than 1

            return _rowManagerTaskOwner.DataSource[column - ColHeaderCount].CurrentDate;
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {
                IDictionary<int, GridRow> settings = (from r in _gridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => _gridRows.IndexOf(k), v => v);

                return settings;
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(_gridRows.OfType<GridRow>()));
            }
        }

        public int MainHeaderRow
        {
            get { return 1; }
        }

        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (string key in _chartSettings.SelectedRows)
            {
                foreach (GridRow gridRow in _gridRows.OfType<GridRow>())
                {
                    if (gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret;
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            if (enabled)
                _chartSettings.SelectedRows.Add(key);
            else
            {
                _chartSettings.SelectedRows.Remove(key);
            }
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public AbstractDetailView Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// Mark weekend days in grid
        /// </summary>
        /// <param name="gridStyleInfo">The grid style info.</param>
        /// <param name="date">The date.</param>
        /// <param name="backColor">if set to <c>true</c> [back color].</param>
        /// <param name="textColor">if set to <c>true</c> [text color].</param>
        private void weekendOrWeekday(GridStyleInfo gridStyleInfo, DateOnly date, bool backColor, bool textColor)
        {
            if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
            {
                if (backColor)
                    gridStyleInfo.BackColor = ColorHolidayCell;
                if (textColor)
                    gridStyleInfo.TextColor = ColorHolidayHeader;
            }
            else
            {
                if (backColor)
                {
                    gridStyleInfo.BackColor = ColorEditableCell;//BackColor;
                }
                gridStyleInfo.TextColor = ForeColor;
            }

            IOutlier outlier;
            if (_outliers.TryGetValue(date, out outlier))
            {
                gridStyleInfo.CellTipText = outlier.Description.Name;
                gridStyleInfo.BackColor = ColorHelper.GridControlOutlierColor();
            }
        }
        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Day; }
        }

        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromDays(1); }
        }

        public override DateTime FirstDate
        {
            get { return _rowManagerTaskOwner.DataSource.First().CurrentDate.Date; }
        }

        public override DateTime LastDate
        {
            get { return _rowManagerTaskOwner.DataSource.Last().CurrentDate.Date; }
        }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            throw new NotImplementedException();
        }

        public IAnnotatable GetAnnotatableObject(int index)
        {
            return _rowManagerTaskOwner.DataSource[index - ColHeaderCount] as IAnnotatable;
        }

        public bool IsAnnotatableCell(int columnIndex, int rowIndex)
        {
            return (columnIndex >= ColHeaderCount && columnIndex < (ColCount + 1)) && (rowIndex == MainHeaderRow);
        }

        protected override void Dispose(bool disposing)
        {
            if (_taskOwnerGridRowMultisiteTemplate != null)
            {
                _taskOwnerGridRowMultisiteTemplate.QueryCellValue -=
                    taskOwnerGridRowMultisiteTemplate_QueryCellValue;
                _taskOwnerGridRowMultisiteTemplate.SaveCellValue -= taskOwnerGridRowMultisiteTemplate_SaveCellValue;
                _taskOwnerGridRowMultisiteTemplate.TemplateSelected -=
                    taskOwnerGridRowMultisiteTemplate_TemplateSelected;
            }
            if (_taskOwnerGridRowSkillTemplate != null)
            {
                _taskOwnerGridRowSkillTemplate.QueryCellValue -= taskOwnerGridRowSkillTemplate_AfterQueryCellInfo;
                _taskOwnerGridRowSkillTemplate.SaveCellValue -= taskOwnerGridRowSkillTemplate_AfterSaveCellInfo;
                _taskOwnerGridRowSkillTemplate.TemplateSelected -= taskOwnerGridRowSkillTemplate_TemplateSelected;
            }
            if (_taskOwnerGridRowWLTemplate != null)
            {
                _taskOwnerGridRowWLTemplate.QueryCellValue -= taskOwnerGridRowWLTemplate_AfterQueryCellInfo;
                _taskOwnerGridRowWLTemplate.SaveCellValue -= taskOwnerGridRowWLTemplate_AfterSaveCellInfo;
                _taskOwnerGridRowWLTemplate.TemplateSelected -= taskOwnerGridRowWLTemplate_TemplateSelected;
            }
            if (_commentMouseController != null)
                _commentMouseController.Dispose();
            if (_gridRows != null)
                _gridRows.Clear();
            if (_outliers != null)
                _outliers.Clear();
            if (_rowManagerMultisiteDay != null)
            {
                _rowManagerMultisiteDay.DataSource.Clear();
                _rowManagerMultisiteDay.Rows.Clear();
            }
            if (_rowManagerTaskOwner != null)
            {
                _rowManagerTaskOwner.DataSource.Clear();
                _rowManagerTaskOwner.Rows.Clear();
            }
            _taskOwnerPeriodHelper = null;
            _owner = null;
            base.Dispose(disposing);
        }
    }
}
