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

		public ChildSkillDetailView()
		{
		}

		internal ChildSkillDetailView(MultisiteSkillDayCalculator skillDayCalculator, IChildSkill skill, ForecasterChartSetting forecasterChartSetting)
			: base(skillDayCalculator, forecasterChartSetting)
		{
			_skill = skill;

			DetailViewLoad();
		}

		public override TemplateTarget TargetType
		{
			get
			{
				return TemplateTarget.Skill;
			}
		}

		public MultisiteSkillDayCalculator MultisiteSkillDayCalculator
		{
			get { return SkillDayCalculator as MultisiteSkillDayCalculator; }
		}

		public IChildSkill Skill
		{
			get { return _skill; }
		}

		private void DetailViewLoad()
		{
			var taskOwnerPeriodHelper = new TaskOwnerHelper(MultisiteSkillDayCalculator.GetVisibleChildSkillDays(_skill));
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

			var taskOwnerDayGridControl = new TaskOwnerDayGridControl(taskOwnerDays, taskOwnerPeriodHelper, this, ForecasterChartSetting.GetChartSettings());
			taskOwnerDayGridControl.Create();
			taskOwnerDayGridControl.Dock = DockStyle.Fill;
			taskOwnerDayGridControl.TemplateSelected += taskOwnerDayGridControlTemplateSelected;
			TabControl.TabPages[2].Tag = WorkingInterval.Day;
			TabControl.TabPages[2].Controls.Add(taskOwnerDayGridControl);
			GridCollection.Add(keyName + WorkingInterval.Day, taskOwnerDayGridControl);

			var skillIntradayGridControl = new SkillIntradayGridControl(taskOwnerDays[0],
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

		private void taskOwnerDayGridControlTemplateSelected(object sender, TemplateEventArgs e)
		{
			TriggerTemplateSelected(e);
		}

		private void skillIntradayGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
		{
			IList<ISkillDataPeriod> dataPeriods = e.DataPeriods.OfType<ISkillDataPeriod>().ToList();
			if (dataPeriods.Count < 1) return;

			var skillDay = dataPeriods[0].Parent as ISkillDay;
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

			var grid = sender as ITaskOwnerGrid;
			if (grid == null) return;
			grid.RefreshGrid();
		}

		  
	}
}
