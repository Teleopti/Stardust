using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autofac;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class BacklogView : Form
	{
		private readonly IComponentContext _container;
		private BacklogModel _model;
		private const int fixedRows = 9;

		public BacklogView(IComponentContext container)
		{
			_container = container;
			InitializeComponent();
			_model = new BacklogModel(container);
			gridControl1.CellModels.Add("TimeSpanLongHourMinutesStatic", new TimeSpanDurationStaticCellModel(gridControl1.Model) { DisplaySeconds = false });
			gridControl1.CellModels.Add("PercentReadOnlyCellModel", new PercentReadOnlyCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("Ignore", new IgnoreCellModel(gridControl1.Model));
			
		}

		private void populateTabControl()
		{
			for (int i = tabControlSkills.TabPages.Count - 1; i >= 1; i--)
			{
				tabControlSkills.TabPages.RemoveAt(i);
			}
			var first = true;
			foreach (var skill in _model.GetTabSkillList())
			{
				if(!first)
				{
					tabControlSkills.TabPages.Add(new TabPage(skill.Name){Tag = skill});
				}

				var page = tabControlSkills.TabPages[tabControlSkills.TabPages.Count - 1];
				page.Text = skill.Name;
				page.Tag = skill;
				first = false;
			}
		}

		private void gridControl1_QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _model.Period().DayCount();
			e.Handled = true;
		}

		private void gridControl1_QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if(toolStripButtonExpand.Checked)
				e.Count = fixedRows + _model.GetIncomingCount((ISkill)tabControlSkills.SelectedTab.Tag);
			else
			{
				e.Count = fixedRows;
			}
			e.Handled = true;
		}

		private void gridControl1_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex == -1 || e.RowIndex == -1)
				return;

			if (e.ColIndex == 0)
			{
				switch (e.RowIndex)
				{
					case 1:
						e.Style.CellValue = "Incoming Demand";
						break;
					case 2:
						e.Style.CellValue = "Forecasted Time";
						break;
					case 3:
						e.Style.CellValue = "Forecasted Backlog";
						break;
					case 4:
						e.Style.CellValue = "Manual Entries";
						break;
					case 5:
						e.Style.CellValue = "Scheduled on skill";
						break;
					case 6:
						e.Style.CellValue = "Scheduled backlog on skill";
						break;
					case 7:
						e.Style.CellValue = "Scheduled on incoming";
						break;
					case 8:
						e.Style.CellValue = "Backlog on incoming";
						break;
					case 9:
						e.Style.CellValue = "SL on incoming";
						break;

				}
			}
			else
			{
				if (e.RowIndex == 0)
				{
					e.Style.CellValue = _model.GetDateForIndex(e.ColIndex);
				}
				else
				{
					TimeSpan time;
					var skill = (ISkill) tabControlSkills.SelectedTab.Tag;
					if (skill == null)
						return;

					switch (e.RowIndex)
					{
						case 1:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							time = _model.GetIncomingForIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							setValue(e, time);
							break;
						case 2:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							time  = _model.GetForecastedForIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							setValue(e, time);
							break;
						case 3:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							e.Style.CellValue = _model.GetForecastedBacklogForIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);						
							break;
						case 4:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							e.Style.CellValue = _model.GetManualEntryOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							e.Style.Borders.Bottom = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.ExtraExtraThick);
							break;
						case 5:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							e.Style.CellValue = _model.GetScheduledOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							break;
						case 6:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							e.Style.CellValue = _model.GetScheduledBacklogOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							break;
						case 7:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							time = _model.GetScheduledOnIncomingIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							setValue(e, time);
							break;
						case 8:
							e.Style.CellType = "TimeSpanLongHourMinutesStatic";
							time = _model.GetScheduledBacklogOnIncomingIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							setValue(e, time);
							break;
						case 9:
							e.Style.CellType = "PercentReadOnlyCellModel";
							e.Style.CellValue = _model.GetScheduledServiceLevelOnIncomingIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag);
							e.Style.Borders.Bottom = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.ExtraExtraThick);
							break;
						default :
							TimeSpan? t = _model.GetScheduledTimeOnTaskForDate(e.ColIndex, e.RowIndex, fixedRows,
								(ISkill) tabControlSkills.SelectedTab.Tag);
							if(!t.HasValue)
							{
								e.Style.CellType = "Ignore";
								e.Style.CellValue = null;
							}
							else
							{
								e.Style.CellType = "TimeSpanLongHourMinutesStatic";
								e.Style.CellValue = t.Value;
							}
							break;

					}
					setBackColor(e);
				}
			}
		}

		private static void setValue(GridQueryCellInfoEventArgs e, TimeSpan time)
		{
			e.Style.CellValue = time > TimeSpan.Zero ? (object) time : string.Empty;
		}

		private void setBackColor(GridQueryCellInfoEventArgs e)
		{
			e.Style.BackColor = Color.White;
			if (_model.IsClosedOnIndex(e.ColIndex, (ISkill) tabControlSkills.SelectedTab.Tag))
				e.Style.BackColor = Color.Khaki;
		}

		private void backlogViewLoad(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Loading...";
			backgroundWorker1.RunWorkerAsync();
			//no code below this row
		}

		private void tabControlSkills_SelectedIndexChanged(object sender, EventArgs e)
		{
			tabControlSkills.SelectedTab.Controls.Add(splitContainer1);
			splitContainer1.Dock = DockStyle.Fill;
			gridControl1.ResetVolatileData();
			gridControl1.Refresh();
			updateChartChart();
		}

		private void toolStripButtonManualEntries_Click(object sender, EventArgs e)
		{
			using (var dialog = new ManualEntryDialog(_model, (ISkill) tabControlSkills.SelectedTab.Tag))
			{
				dialog.ShowDialog(this);
			}
			gridControl1.Invalidate();
		}

		private void toolStripButtonSave_Click(object sender, EventArgs e)
		{
			_model.TransferSkillDays((ISkill)tabControlSkills.SelectedTab.Tag);
		}

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			_model.Load(backgroundWorker1);
		}

		private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			var text = e.UserState as string;
			if(text != null)
				toolStripStatusLabel1.Text = text;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			populateTabControl();
			gridControl1.ControllerOptions = GridControllerOptions.All & (~GridControllerOptions.OleDataSource);
			gridControl1.ResetVolatileData();
			gridControl1.BeginUpdate();
			gridControl1.ColWidths.SetSize(0, 140);
			gridControl1.RowHeights.SetSize(0, 30);
			gridControl1.EndUpdate(true);
			chart1.Palette = ChartColorPalette.SeaGreen;
			updateChartChart();
			toolStripStatusLabel1.Text = string.Empty;
		}

		private void toolStripButtonSkillMapper_Click(object sender, EventArgs e)
		{
			using (var dialog = new SkillMapperDialog(_model.SkillPairs))
			{
				dialog.ShowDialog(this);
			}
		}

		private void updateChartChart()
		{
			chart1.Series.Clear();
			var series1 = chart1.Series.Add("Scheduled on incoming");
			var series2 = chart1.Series.Add("Backlog on incoming");
			series1.ChartType = SeriesChartType.StackedColumn;
			series2.ChartType = SeriesChartType.StackedColumn;
			for (int i = 1; i < gridControl1.ColCount; i++)
			{
				var datapoint = new DataPoint(i,
					_model.GetScheduledOnIncomingIndex(i, (ISkill) tabControlSkills.SelectedTab.Tag).TotalHours);
				series1.Points.Add(datapoint);
				datapoint = new DataPoint(i,
					_model.GetScheduledBacklogOnIncomingIndex(i, (ISkill)tabControlSkills.SelectedTab.Tag).TotalHours);
				series2.Points.Add(datapoint);
			}
		}

		private void toolStripButtonExpand_Click(object sender, EventArgs e)
		{
			toolStripButtonExpand.Checked = !toolStripButtonExpand.Checked;
			gridControl1.ResetVolatileData();
			gridControl1.Refresh();
		}

		private void buttonLoad_Click(object sender, EventArgs e)
		{
			_model = new BacklogModel(_container);
			backlogViewLoad(this, new EventArgs());
		}
	}
}
 