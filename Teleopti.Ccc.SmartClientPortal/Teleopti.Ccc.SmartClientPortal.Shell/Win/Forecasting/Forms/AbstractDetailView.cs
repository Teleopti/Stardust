using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.Win.Forecasting.Forms;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public partial class AbstractDetailView : BaseUserControl
    {
        private readonly SkillDayCalculator _skillDayCalculator;
        private readonly IDictionary<string, TeleoptiGridControl> _gridCollection = new Dictionary<string, TeleoptiGridControl>();

        private DateOnly _currentDay;
        private TimeSpan _currentTimeOfDay;
        
        protected ForecasterChartSetting ForecasterChartSetting { get; private set; }
        
        /// <summary>
        /// Gets the skill day calculator.
        /// </summary>
        /// <value>The skill day calculator.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-27
        /// </remarks>
        protected SkillDayCalculator SkillDayCalculator
        {
            get { return _skillDayCalculator; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDetailView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        protected AbstractDetailView()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();
        }

        private void SetColors()
        {
            tabControl.BackColor = ColorHelper.TabBackColor();
            BackColor = ColorHelper.TabBackColor();

            tabPageMonth.Tag = WorkingInterval.Month;
            tabPageIntraday.Tag = WorkingInterval.Intraday;
            tabPageDay.Tag = WorkingInterval.Day;
            tabPageWeek.Tag = WorkingInterval.Week;
            TabControl.ItemSize = new Size(0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDetailView"/> class.
        /// </summary>
        /// <param name="skillDayCalculator">The skill day calculator.</param>
        /// <param name="forecasterChartSetting">The forecaster chart setting.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        protected AbstractDetailView(SkillDayCalculator skillDayCalculator, ForecasterChartSetting forecasterChartSetting)
            : this()
        {
            ForecasterChartSetting = forecasterChartSetting;
            _skillDayCalculator = skillDayCalculator;
            TabControl.ItemSize = new Size(1, 1);

			_skillDayCalculator.DaysUnsaved += skillDayCalculatorDaysUnsaved;
        }

        /// <summary>
        /// Gets or sets the current skill day.
        /// </summary>
        /// <value>The current skill day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public DateOnly CurrentDay
        {
            get { return _currentDay; }
            set {
                if (_currentDay != value)
                {
                    _currentDay = value;
                    OnCurrentDayChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current time of day.
        /// </summary>
        /// <value>The current time of day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-14
        /// </remarks>
        public TimeSpan CurrentTimeOfDay
        {
            get { return _currentTimeOfDay; }
            set
            {
                if (_currentTimeOfDay != value)
                {
                    _currentTimeOfDay = value;
                    OnCurrentTimeOfDayChanged();
                }
            }
        }

        private void OnCurrentTimeOfDayChanged()
        {
            foreach (TabPageAdv tab in tabControl.TabPages)
            {
                foreach (ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tab))
                {
                    BaseIntradayGridControl intradayGridControl = taskOwnerGrid as BaseIntradayGridControl;
                    if (intradayGridControl!=null)
                    intradayGridControl.GoToTime(_currentTimeOfDay);
                }
            }

            triggerWorkingIntervalChanged(new WorkingIntervalChangedEventArgs
            {
                NewWorkingInterval = CurrentWorkingInterval,
                NewStartDate = _currentDay,
                NewTimeOfDay = _currentTimeOfDay
            });
        }

        /// <summary>
        /// Called when [current day changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        private void OnCurrentDayChanged()
        {
            foreach (TabPageAdv tab in tabControl.TabPages)
            {
                foreach(ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tab))
                {
                    taskOwnerGrid.GoToDate(_currentDay);
                }
            }
            //Update the other subscribers
            triggerWorkingIntervalChanged(new WorkingIntervalChangedEventArgs
            {
                NewWorkingInterval = CurrentWorkingInterval,
                NewStartDate = _currentDay
            });
        }

        /// <summary>
        /// Gets the grid collection.
        /// </summary>
        /// <value>The grid collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        public IDictionary<string, TeleoptiGridControl> GridCollection
        {
            get { return _gridCollection; }
        }

        /// <summary>
        /// Gets the type of the skill.
        /// </summary>
        /// <value>The type of the skill.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-26
        /// </remarks>
        public ISkillType SkillType
        {
            get { return _skillDayCalculator.Skill.SkillType; }
        }

        /// <summary>
        /// Shows the tab.
        /// </summary>
        /// <param name="workingInterval">The working interval.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void ShowTab(WorkingInterval workingInterval)
        {
            TabPageAdv tabPage = tabControl.TabPages.OfType<TabPageAdv>().FirstOrDefault(t => (WorkingInterval)t.Tag == workingInterval);
            if (tabPage != null && tabControl.SelectedTab!=tabPage) tabControl.SelectedTab = tabPage;
        }

        /// <summary>
        /// Refreshes the current tab.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-29
        /// </remarks>
        public void RefreshCurrentTab()
        {
            foreach (ITaskOwnerGrid taskOwnerGrid in GetGridsInControl(tabControl.SelectedTab))
            {
                taskOwnerGrid.RefreshGrid();
            }
        }

        public void RefreshIntradayBehindCurrentTab()
        {
            if ((WorkingInterval)tabControl.SelectedTab.Tag == WorkingInterval.Intraday)
                return;
            foreach (var tab in tabControl.TabPages.OfType<TabPageAdv>())
            {
                if ((WorkingInterval)tab.Tag == WorkingInterval.Intraday)
                    foreach (ITaskOwnerGrid taskOwnerGrid in GetGridsInControl(tab))
                    {
                        taskOwnerGrid.RefreshGrid();
                    }
            }
        }

        /// <summary>
        /// Gets the grids on tab.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        internal static IList<ITaskOwnerGrid> GetGridsInControl(Control parentControl)
        {
            List<ITaskOwnerGrid> gridControls = new List<ITaskOwnerGrid>();
            foreach (Control control in parentControl.Controls)
            {
                gridControls.AddRange(GetChildControlGrids(control));
            }
            return gridControls;
        }

        private static IList<ITaskOwnerGrid> GetChildControlGrids(Control parentControl)
        {
            List<ITaskOwnerGrid> gridControls = new List<ITaskOwnerGrid>();
            ITaskOwnerGrid gridControl = parentControl as ITaskOwnerGrid;
            if (gridControl != null)
            {
                gridControls.Add(gridControl);
            }
            else
            {
                foreach (Control control in parentControl.Controls)
                {
                    gridControl = control as ITaskOwnerGrid;
                    if (gridControl != null)
                        gridControls.Add(gridControl);
                    else
                        gridControls.AddRange(GetChildControlGrids(control));
                }
            }
            return gridControls;
        }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual TemplateTarget TargetType
        {
            get { return TemplateTarget.Workload; }
        }

		private void skillDayCalculatorDaysUnsaved(object sender, CustomEventArgs<IUnsavedDaysInfo> e)
		{
			TriggerChangeUnsavedDaysStyle(e);
		}

        private void triggerWorkingIntervalChanged(WorkingIntervalChangedEventArgs eventArgs)
        {
        	var handler = WorkingIntervalChanged;
            if (handler != null)
            {
                handler.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Triggers the values changed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-29
        /// </remarks>
        public void TriggerValuesChanged()
        {
        	var handler = ValuesChanged;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the tab control.
        /// </summary>
        /// <value>The tab control.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        protected TabControlAdv TabControl
        {
            get { return tabControl; }
        }

        /// <summary>
        /// Gets or sets the current grid row.
        /// </summary>
        /// <value>The current grid row.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        public GridRow CurrentGridRow
        {
            get; private set;
        }

        /// <summary>
        /// Occurs when [working interval changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        public event EventHandler<WorkingIntervalChangedEventArgs> WorkingIntervalChanged;

		public event EventHandler<CustomEventArgs<IUnsavedDaysInfo>> ChangeUnsavedDaysStyle;

    	protected void TriggerChangeUnsavedDaysStyle(CustomEventArgs<IUnsavedDaysInfo> e)
    	{
    		var handler = ChangeUnsavedDaysStyle;
    		if (handler != null) handler(this, e);
    	}

    	/// <summary>
        /// Occurs when [values changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-29
        /// </remarks>
        public event EventHandler<EventArgs> ValuesChanged;

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tabControl.SelectedTab))
            {
                taskOwnerGrid.RefreshGrid();
            }

            triggerWorkingIntervalChanged(new WorkingIntervalChangedEventArgs
            {
                NewWorkingInterval = CurrentWorkingInterval,
                NewStartDate = _currentDay,
                NewTimeOfDay = _currentTimeOfDay
            });
        }

        /// <summary>
        /// Gets the current working interval.
        /// </summary>
        /// <value>The current working interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        public WorkingInterval CurrentWorkingInterval
        {
            get { return (WorkingInterval) tabControl.SelectedTab.Tag; }   
        }

        public event EventHandler<EventArgs> CellClicked;

        public void TriggerCellClicked(ITaskOwnerGrid grid, GridRow row)
        {
            CurrentGrid = grid;
            CurrentGridRow = row;

        	var handler = CellClicked;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public ITaskOwnerGrid CurrentGrid { get; private set; }

        /// <summary>
        /// Sets the template.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="templateTarget">The template target.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-09
        /// </remarks>
        public void SetTemplate(string templateName, TemplateTarget templateTarget)
        {
            if (CurrentWorkingInterval != WorkingInterval.Day) return;

            foreach (ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tabControl.SelectedTab))
            {
                TaskOwnerDayGridControl gridControl = taskOwnerGrid as TaskOwnerDayGridControl;
                if (gridControl != null)
                {
                    gridControl.SetTemplate(templateName, templateTarget);
                    break;
                }
            }
        }

        /// <summary>
        /// Resets the templates.
        /// </summary>
        /// <param name="templateTarget">The template target.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-15
        /// </remarks>
        public void ResetTemplates(TemplateTarget templateTarget)
        {
            if (CurrentWorkingInterval != WorkingInterval.Day) return;

            foreach (ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tabControl.SelectedTab))
            {
                TaskOwnerDayGridControl gridControl = taskOwnerGrid as TaskOwnerDayGridControl;
                if (gridControl != null)
                {
                    gridControl.ResetTemplates(templateTarget);
                    break;
                }
            }
        }

        public void ResetLongterm()
        {
            if (CurrentWorkingInterval != WorkingInterval.Day) return;

            foreach (ITaskOwnerGrid taskOwnerGrid in GetChildControlGrids(tabControl.SelectedTab))
            {
                TaskOwnerDayGridControl gridControl = taskOwnerGrid as TaskOwnerDayGridControl;
                if (gridControl != null)
                {
                    gridControl.ResetLongterm();
                    break;
                }
            }
        }

        public event EventHandler<TemplateEventArgs> TemplateSelected;

        protected void TriggerTemplateSelected(TemplateEventArgs eventArgs)
        {
        	var handler = TemplateSelected;
            if (handler!= null)
            {
                handler.Invoke(
                    this, eventArgs);
            }
        }

        private void ReleaseManagedResources()
        {
            foreach (TeleoptiGridControl gridControl in _gridCollection.Values)
            {
                gridControl.Dispose();
            }
            _gridCollection.Clear();
            _skillDayCalculator.ClearSkillStaffPeriods();
        }

        private void UnhookEvents()
        {
            tabControl.SelectedIndexChanged -= tabControl_SelectedIndexChanged;
        }
    }
}
