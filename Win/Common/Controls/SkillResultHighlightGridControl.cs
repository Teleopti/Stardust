using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls
{
	public partial class SkillResultHighlightGridControl : GridControl, IHelpContext
	{
		private const int rowCount = 6;
		public SkillResultHighlightGridControl()
		{
			InitializeComponent();
			initializeGrid();
		}

		public event EventHandler<GoToDateEventArgs> GoToDate;

		public void OnGoToDate(GoToDateEventArgs e)
		{
			EventHandler<GoToDateEventArgs> handler = GoToDate;
			
			if (handler != null) 
				handler(this, e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxMost"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLowest"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLeast"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxHeighest"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "STDev"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Grid.GridStyleInfo.set_Text(System.String)")]
		private void initializeGrid()
		{
			Name = "SkillResultHighlight";
			
			BackColor = ColorHelper.GridControlGridInteriorColor();
			Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
			GridHelper.GridStyle(this);

			RowCount = rowCount;
			NumberedRowHeaders = true;
			NumberedColHeaders = false;
			DefaultColWidth = 150;
			ColCount = 4;
			this[0, 1].Text = "xxLeast staffed";
			this[0, 2].Text = "xxLowest ESL";
			this[0, 3].Text = "xxHeighest intraday STDev";
			this[0, 4].Text = "xxMost staffed";
			//GridStyleInfo.Default.CellType = "Static";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			base.OnQueryCellInfo(e);
			if(e.RowIndex > 0 && e.ColIndex > 0)
				e.Style.CellType = "Static";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Grid.GridStyleInfo.set_Text(System.String)")]
		public void DrawGridContents(ISchedulerStateHolder stateHolder, ISkill currentSkill)
		{
			for (int i = 0; i < rowCount; i++)
			{
				this[i + 1, 1].Text = "";
				this[i + 1, 2].Text = "";
				this[i + 1, 3].Text = "";
				this[i + 1, 4].Text = "";
				this[i + 1, 1].Tag = null;
				this[i + 1, 2].Tag = null;
				this[i + 1, 3].Tag = null;
				this[i + 1, 4].Tag = null;
			}
			var staffings = new List<ValueAndDatePair>();
			var esl = new List<ValueAndDatePair>();
			var stDev = new List<ValueAndDatePair>();	
			foreach (var skillDay in stateHolder.SchedulingResultState.SkillDaysOnDateOnly(stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()))
			{
				if(skillDay.Skill != currentSkill)
					continue;

				double? relativeDiff = SkillStaffPeriodHelper.RelativeDifference(skillDay.SkillStaffPeriodCollection);
				if(relativeDiff.HasValue)
				{
					staffings.Add(new ValueAndDatePair(relativeDiff.Value, skillDay.CurrentDate));
					esl.Add(new ValueAndDatePair(SkillStaffPeriodHelper.EstimatedServiceLevel(skillDay.SkillStaffPeriodCollection).Value, skillDay.CurrentDate));
					stDev.Add(new ValueAndDatePair(SkillStaffPeriodHelper.SkillDayGridSmoothness(skillDay.SkillStaffPeriodCollection).Value, skillDay.CurrentDate));
				}
					
			}

			staffings.Sort();
			CultureInfo culture = TeleoptiPrincipal.Current.Regional.Culture;
			for (int i = 0; i < rowCount; i++)
			{
				if (i < staffings.Count)
				{
					string staffing = staffings[i].Value.ToString("###%", culture);
					string s = String.Format("{0, 5}", staffing);
					this[i + 1, 1].Text = s + "  (" + staffings[i].Date.ToShortDateString(culture) + ")";
					this[i + 1, 1].Tag = staffings[i].Date;
				}

			}

			int row = 1;
			for (int i = staffings.Count - 1; i >= staffings.Count - rowCount; i--)
			{
				if (i < staffings.Count && i > -1)
				{
					string staffing = staffings[i].Value.ToString("###%", culture);
					string s = String.Format("{0, 5}", staffing);
					this[row, 4].Text = s + "  (" + staffings[i].Date.ToShortDateString(culture) + ")";
					this[row, 4].Tag = staffings[i].Date;
				}

				row++;
			}

			stDev.Sort();
			row = 1;
			for (int i = stDev.Count - 1; i >= stDev.Count - rowCount; i--)
			{
				if (i < stDev.Count && i > -1)
				{
					string dev = Math.Round(stDev[i].Value, 2).ToString("0.00", culture);
					string s = String.Format("{0, 5}", dev);
					this[row, 3].Text = s + "  (" + stDev[i].Date.ToShortDateString(culture) + ")";
					this[row, 3].Tag = stDev[i].Date;
				}

				row++;
			}

			esl.Sort();
			for (int i = 0; i < rowCount; i++)
			{
				if (i < esl.Count)	
				{
					string eslvalue = new Percent(esl[i].Value).ToString(culture);
					string s = String.Format("{0, 5}", eslvalue);
					this[i + 1, 2].Text = s + "  (" + esl[i].Date.ToShortDateString(culture) + ")";
					this[i + 1, 2].Tag = esl[i].Date; 
				}
			}
		}

		public bool HasHelp
		{
			get { return false; }
		}

		public string HelpId
		{
			get { return Name; }
		}

		private void toolStripMenuItemGoToDate_Click(object sender, EventArgs e)
		{
			var currentCell = this.CurrentCell;
			if (currentCell == null)
				return;

			int col;
			int row;
			currentCell.GetCurrentCell(out row, out col);
			var tag = this[row, col].Tag;
			if (tag == null)
				return;

			GoToDateEventArgs args = new GoToDateEventArgs((DateOnly)tag);
			OnGoToDate(args);
		}
	}

	internal class ValueAndDatePair : IComparable<ValueAndDatePair>
	{
		public double Value { get; set; }
		public DateOnly Date { get; set; }

		public ValueAndDatePair(double value, DateOnly date)
		{
			Value = value;
			Date = date;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public int CompareTo(ValueAndDatePair other)
		{
			if (Value < other.Value)
				return -1;

			if (Value > other.Value)
				return 1;

			return 0;
		}
	}

	public class GoToDateEventArgs : EventArgs
	{
		private readonly DateOnly _date;

		public GoToDateEventArgs(DateOnly date)
		{
			_date = date;
		}

		public DateOnly Date
		{
			get { return _date; }
		}
	}
}
