using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class MultisiteSkillDetailView : AbstractDetailView
    {
	    private IStatisticHelper _statisticsHelper;

	    /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteSkillDetailView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public MultisiteSkillDetailView(IStatisticHelper statisticsHelper)
	    {
		    _statisticsHelper = statisticsHelper;
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteSkillDetailView"/> class.
        /// </summary>
        /// <param name="skillDayCalculator">The skill day calculator.</param>
        /// <param name="forecasterChartSetting">The forecaster chart setting.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        internal MultisiteSkillDetailView(MultisiteSkillDayCalculator skillDayCalculator, ForecasterChartSetting forecasterChartSetting, IStatisticHelper statisticsHelper)
            : base(skillDayCalculator, forecasterChartSetting)
	    {
		    _statisticsHelper = statisticsHelper;
		    DetailViewLoad();
	    }

	    /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public override TemplateTarget TargetType
        {
            get
            {
                return TemplateTarget.Skill;
            }
        }

        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-09
        /// </remarks>
        public IMultisiteSkill Skill
        {
            get { return MultisiteSkillDayCalculator.MultisiteSkill; }
        }

        /// <summary>
        /// Gets the multisite skill day calculator.
        /// </summary>
        /// <value>The multisite skill day calculator.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-04
        /// </remarks>
        public MultisiteSkillDayCalculator MultisiteSkillDayCalculator
        {
            get { return SkillDayCalculator as MultisiteSkillDayCalculator; }
        }

        private void DetailViewLoad()
        {
            TaskOwnerHelper taskOwnerPeriodHelper = new TaskOwnerHelper(SkillDayCalculator.VisibleSkillDays);
            IList<TaskOwnerPeriod> taskOwnerWeeks = taskOwnerPeriodHelper.CreateWholeWeekTaskOwnerPeriods();
            IList<TaskOwnerPeriod> taskOwnerMonths = taskOwnerPeriodHelper.CreateWholeMonthTaskOwnerPeriods();
            IList<ITaskOwner> taskOwnerDays = taskOwnerPeriodHelper.TaskOwnerDays;

            string keyName = Skill.Id + ".Skill.";

            Control control = new TaskOwnerMonthGridControl(
                taskOwnerMonths.OfType<ITaskOwner>(),
                this, ForecasterChartSetting.GetChartSettings());
            control.Dock = DockStyle.Fill;
            TabControl.TabPages[0].Tag = WorkingInterval.Month;
            TabControl.TabPages[0].Controls.Add(control);
            GridCollection.Add(keyName + WorkingInterval.Month, (TeleoptiGridControl)control);

            control = new TaskOwnerWeekGridControl(
                taskOwnerWeeks.OfType<ITaskOwner>(),
                this, ForecasterChartSetting.GetChartSettings());
            control.Dock = DockStyle.Fill;
            TabControl.TabPages[1].Tag = WorkingInterval.Week;
            TabControl.TabPages[1].Controls.Add(control);
            GridCollection.Add(keyName + WorkingInterval.Week, (TeleoptiGridControl)control);

            IList<IMultisiteDay> multisiteDays = new List<IMultisiteDay>(MultisiteSkillDayCalculator.VisibleMultisiteDays);
            TaskOwnerDayGridControl taskOwnerDayGridControl = new TaskOwnerDayGridControl(taskOwnerDays,multisiteDays,
                taskOwnerPeriodHelper, this, ForecasterChartSetting.GetChartSettings(),_statisticsHelper);
            taskOwnerDayGridControl.Create();
            taskOwnerDayGridControl.Dock = DockStyle.Fill;
            taskOwnerDayGridControl.TemplateSelected += taskOwnerDayGridControl_TemplateSelected;
            TabControl.TabPages[2].Tag = WorkingInterval.Day;
            TabControl.TabPages[2].Controls.Add(taskOwnerDayGridControl);
            GridCollection.Add(keyName + WorkingInterval.Day, taskOwnerDayGridControl);

            SkillIntradayGridControl skillIntradayGridControl = new SkillIntradayGridControl(
                taskOwnerDays[0], multisiteDays,taskOwnerPeriodHelper,
                Skill, Skill.TimeZone, Skill.DefaultResolution, this, ForecasterChartSetting.GetChartSettings());

            skillIntradayGridControl.Create();

            skillIntradayGridControl.ModifyCells += skillIntradayGridControl_ModifyCells;
            skillIntradayGridControl.Dock = DockStyle.Fill;
            TabControl.TabPages[3].Tag = WorkingInterval.Intraday;
            TabControl.TabPages[3].Controls.Add(skillIntradayGridControl);
            GridCollection.Add(keyName + WorkingInterval.Intraday, skillIntradayGridControl);

            foreach (TabPageAdv tabPage in TabControl.TabPages)
            {
                foreach (ITaskOwnerGrid taskOwnerGrid in GetGridsInControl(tabPage))
                {
                    if (!taskOwnerGrid.HasColumns &&
                        ((WorkingInterval)tabPage.Tag) != WorkingInterval.Intraday)
                    {
                        TabControl.TabPages.Remove(tabPage);
                        break;
                    }
                }
            }

            TabControl.ItemSize = new System.Drawing.Size(1, 0);
        }

        private void taskOwnerDayGridControl_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void skillIntradayGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
            IList<ISkillDataPeriod> dataPeriods = e.DataPeriods.OfType<ISkillDataPeriod>().ToList();
            if (dataPeriods.Count > 0)
            {
                ISkillDay skillDay = dataPeriods[0].Parent as ISkillDay;
                if (skillDay == null) return;

                switch (e.ModifyCellOption)
                {
                    case ModifyCellOption.Merge:
                        if (dataPeriods.Count == 1) return;
                        skillDay.MergeSkillDataPeriods(dataPeriods);
                        break;
                    case ModifyCellOption.Split:
                        skillDay.SplitSkillDataPeriods(dataPeriods);
                        break;
                }
            }

            IList<IMultisitePeriod> multisitePeriods = e.DataPeriods.OfType<IMultisitePeriod>().ToList();
            if (multisitePeriods.Count > 0)
            {
                IMultisiteDay multisiteDay = multisitePeriods[0].Parent as IMultisiteDay;
                if (multisiteDay == null) return;

                switch (e.ModifyCellOption)
                {
                    case ModifyCellOption.Merge:
                        if (multisitePeriods.Count == 1) return;
                        multisiteDay.MergeMultisitePeriods(multisitePeriods);
                        break;
                    case ModifyCellOption.Split:
                        multisiteDay.SplitMultisitePeriods(multisitePeriods);
                        break;
                }
            }

            if (dataPeriods.Count > 0 ||
                multisitePeriods.Count > 0)
            {
                ITaskOwnerGrid grid = sender as ITaskOwnerGrid;
                if (grid == null) return;
                grid.RefreshGrid();
            }
        }
    }
}
