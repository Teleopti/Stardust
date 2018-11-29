using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
	public partial class MultisiteDistributions : BaseUserControl, IPropertyPage
	{
		public MultisiteDistributions()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();

			gridSubSkills.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			gridSubSkills.CutPaste.ClipboardFlags |= GridDragDropFlags.NoAppendRows;
		}

		public void Populate(IAggregateRoot aggregateRoot)
		{
			var skill = aggregateRoot as IMultisiteSkill;
			loadChildSkills(skill);
		}

		public bool Depopulate(IAggregateRoot aggregateRoot)
		{
			var skill = aggregateRoot as IMultisiteSkill;
			return getDistributions(skill);
		}

		private bool getDistributions(IMultisiteSkill skill)
		{
			decimal percentageSum = 0;
			var distributions = new Dictionary<IChildSkill, Percent>();
			var childSkillLookup = skill.ChildSkills.ToLookup(c => c.Id.Value);
			for (int i = 1; i < gridSubSkills.RowCount; i++)
			{
				var cell = gridSubSkills[i, 2];
				var id = (Guid)cell.Tag;
				var child = childSkillLookup[id].FirstOrDefault();
				if (child != null)
				{
					var value = (Percent)cell.CellValue;
					distributions.Add(child, value);
					percentageSum += (decimal)value.Value;
				}
			}

			DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);
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
			if (percentageSum != 1m)
			{
				var warn =
					new TimedWarningDialog(3, UserTexts.Resources.SubskillDistributionMustSumToHundredPercent, gridSubSkills);
				warn.ShowDialog(this);
			}

			return percentageSum == 1m;
		}

		public string PageName => UserTexts.Resources.SubSkillDistributions;

		public void SetEditMode()
		{
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			gridSubSkills.Refresh();
		}

		#region Prepare grid

		private void loadChildSkills(IMultisiteSkill skill)
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
				gridSubSkills[i, 1].ReadOnly = true;
				gridSubSkills[i, 2].Text = UserTexts.Resources.Percentage;
				gridSubSkills[i, 2].ReadOnly = true;

				double[] percent = MultisiteHelper.CalculateLowVarianceDistribution(100, childCount, 2);

				for (i = 1; i <= childCount; i++)
				{
					gridSubSkills[i, 1].CellType = "Header";
					gridSubSkills[i, 1].Text = skill.ChildSkills[i - 1].Name;
					gridSubSkills[i, 1].ReadOnly = true;

					gridSubSkills[i, 2].CellType = "MultiSitePercentCell";
					gridSubSkills[i, 2].CellValue = new Percent(percent[i - 1]);
					gridSubSkills[i, 2].Tag = skill.ChildSkills[i - 1].Id.Value;
					sum += percent[i - 1];
				}
				gridSubSkills[i, 1].CellType = "Header";
				gridSubSkills[i, 1].Text = UserTexts.Resources.Total;
				gridSubSkills[i, 1].ReadOnly = true;

				gridSubSkills[i, 2].CellType = "PercentReadOnly";
				gridSubSkills[i, 2].CellValue = new Percent(sum);
				gridSubSkills[i, 2].HorizontalAlignment = GridHorizontalAlignment.Right;
			}
			gridSubSkills.Model.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			gridSubSkills.Refresh();
			gridSubSkills.CurrentCellEditingComplete += gridSubSkillsCurrentCellEditingComplete;
		}

		void gridSubSkillsCurrentCellEditingComplete(object sender, EventArgs e)
		{
			gridSubSkills[gridSubSkills.RowCount, 2].CellValue = new Percent(total());
		}

		private double total()
		{
			double percentageSum = 0;
			for (int i = 1; i < gridSubSkills.RowCount; i++)
			{
				GridStyleInfo cell = gridSubSkills[i, 2];
				var value = (Percent)cell.CellValue;
				percentageSum += value.Value;
			}
			return percentageSum;
		}

		//todo this page needs a sum row which is updated by value changed or something...

		private GridCellModelBase initializeMultiSitePercentCell()
		{
			var cellModel = new PercentCellModel(gridSubSkills.Model)
			{
				NumberOfDecimals = 2,
				MinMax = new MinMax<double>(0.00, 1)
			};
			return cellModel;
		}

		private GridCellModelBase initializePercentReadOnlyCell()
		{
			var cellModel = new PercentReadOnlyCellModel(gridSubSkills.Model) {NumberOfDecimals = 2};
			return cellModel;
		}

		#endregion
	}
}
