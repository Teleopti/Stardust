using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	public class SkillDetailView : AbstractDetailView
	{
		private readonly ISkill _skill;
		private TaskOwnerDayGridControl _taskOwnerDayGridControl;
		private SkillIntradayGridControl _skillIntradayGridControl;

		public SkillDetailView()
		{
		}

		internal SkillDetailView(SkillDayCalculator skillDayCalculator, ISkill skill, ForecasterChartSetting forecasterChartSetting)
			: base(skillDayCalculator,forecasterChartSetting)
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

		public ISkill Skill
		{
			get { return _skill; }
		}

		private void DetailViewLoad()
		{
			var taskOwnerPeriodHelper = new TaskOwnerHelper(SkillDayCalculator.VisibleSkillDays);
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

			_taskOwnerDayGridControl = new TaskOwnerDayGridControl(taskOwnerDays, taskOwnerPeriodHelper, this,
																   ForecasterChartSetting.GetChartSettings());
			_taskOwnerDayGridControl.Create();
			_taskOwnerDayGridControl.Dock = DockStyle.Fill;
			_taskOwnerDayGridControl.TemplateSelected += taskOwnerDayGridControlTemplateSelected;

			TabControl.TabPages[2].Tag = WorkingInterval.Day;
			TabControl.TabPages[2].Controls.Add(_taskOwnerDayGridControl);
			GridCollection.Add(keyName + WorkingInterval.Day, _taskOwnerDayGridControl);

			_skillIntradayGridControl = new SkillIntradayGridControl(taskOwnerDays[0],
																							 taskOwnerPeriodHelper,
																							 _skill.TimeZone,
																							 _skill.DefaultResolution,
																							 this,
																							 ForecasterChartSetting.
																								 GetChartSettings());
			_skillIntradayGridControl.Create();
			_skillIntradayGridControl.ModifyCells += skillIntradayGridControlModifyCells;
			_skillIntradayGridControl.Dock = DockStyle.Fill;
			TabControl.TabPages[3].Tag = WorkingInterval.Intraday;
			TabControl.TabPages[3].Controls.Add(_skillIntradayGridControl);
			GridCollection.Add(keyName + WorkingInterval.Intraday, _skillIntradayGridControl);

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

		private void taskOwnerDayGridControlTemplateSelected(object sender, TemplateEventArgs e)
		{
			TriggerTemplateSelected(e);
		}

		private static void skillIntradayGridControlModifyCells(object sender, ModifyCellEventArgs e)
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
				var grid = sender as ITaskOwnerGrid;
				if (grid == null) return;
				grid.RefreshGrid();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				unhookEvents();
			}
			base.Dispose(disposing);
		}

		private void unhookEvents()
		{
			_skillIntradayGridControl.ModifyCells -= skillIntradayGridControlModifyCells;
			_taskOwnerDayGridControl.TemplateSelected -= taskOwnerDayGridControlTemplateSelected;
		}
	}
}
