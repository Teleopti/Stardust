using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class ChildSkillDetailView : AbstractDetailView
    {
        private readonly IChildSkill _skill;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDetailView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public ChildSkillDetailView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDetailView"/> class.
        /// </summary>
        /// <param name="skillDayCalculator">The skill day calculator.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="forecasterChartSetting">The forecaster chart setting.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        internal ChildSkillDetailView(MultisiteSkillDayCalculator skillDayCalculator, IChildSkill skill, ForecasterChartSetting forecasterChartSetting)
            : base(skillDayCalculator, forecasterChartSetting)
        {
            _skill = skill;

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

        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-09
        /// </remarks>
        public IChildSkill Skill
        {
            get { return _skill; }
        }

        private void DetailViewLoad()
        {
            TaskOwnerHelper taskOwnerPeriodHelper = new TaskOwnerHelper(MultisiteSkillDayCalculator.GetVisibleChildSkillDays(_skill));
            IList<TaskOwnerPeriod> taskOwnerWeeks = taskOwnerPeriodHelper.CreateWholeWeekTaskOwnerPeriods();
            IList<TaskOwnerPeriod> taskOwnerMonths = taskOwnerPeriodHelper.CreateWholeMonthTaskOwnerPeriods();
            IList<ITaskOwner> taskOwnerDays = taskOwnerPeriodHelper.TaskOwnerDays;

            string keyName = _skill.Id + ".Skill.";

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

            TaskOwnerDayGridControl taskOwnerDayGridControl = new TaskOwnerDayGridControl(taskOwnerDays, taskOwnerPeriodHelper, this, ForecasterChartSetting.GetChartSettings());
            taskOwnerDayGridControl.Create();
            taskOwnerDayGridControl.Dock = DockStyle.Fill;
            taskOwnerDayGridControl.TemplateSelected += taskOwnerDayGridControl_TemplateSelected;
            TabControl.TabPages[2].Tag = WorkingInterval.Day;
            TabControl.TabPages[2].Controls.Add(taskOwnerDayGridControl);
            GridCollection.Add(keyName + WorkingInterval.Day, taskOwnerDayGridControl);

            SkillIntradayGridControl skillIntradayGridControl = new SkillIntradayGridControl(taskOwnerDays[0],
                                                                                             taskOwnerPeriodHelper,
                                                                                             _skill.TimeZone,
                                                                                             _skill.DefaultResolution,
                                                                                             this,
                                                                                             ForecasterChartSetting.
                                                                                                 GetChartSettings());
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
        }

        private void taskOwnerDayGridControl_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void skillIntradayGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
            IList<ISkillDataPeriod> dataPeriods = e.DataPeriods.OfType<ISkillDataPeriod>().ToList();
            if (dataPeriods.Count < 1) return;

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

            ITaskOwnerGrid grid = sender as ITaskOwnerGrid;
            if (grid == null) return;
            grid.RefreshGrid();
        }

		  private void InitializeComponent()
		  {
			this.SuspendLayout();
			// 
			// tabPageMonth
			// 
			this.tabPageMonth.Location = new System.Drawing.Point(3, 24);
			this.tabPageMonth.Size = new System.Drawing.Size(462, 307);
			// 
			// tabPageWeek
			// 
			this.tabPageWeek.Location = new System.Drawing.Point(3, 24);
			this.tabPageWeek.Size = new System.Drawing.Size(462, 307);
			// 
			// tabPageDay
			// 
			this.tabPageDay.Location = new System.Drawing.Point(3, 24);
			this.tabPageDay.Size = new System.Drawing.Size(462, 307);
			// 
			// tabPageIntraday
			// 
			this.tabPageIntraday.Location = new System.Drawing.Point(3, 24);
			this.tabPageIntraday.Size = new System.Drawing.Size(462, 307);
			// 
			// ChildSkillDetailView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.BackColor = System.Drawing.Color.White;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ChildSkillDetailView";
			this.ResumeLayout(false);

		  }
    }
}
