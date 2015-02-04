using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autofac;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class BacklogView : Form
	{
		private readonly IComponentContext _container;
		private readonly DateOnlyPeriod _period;
		private BacklogModel _model;
		private int _fixedRows;
		private IList<IBacklogGridRow> _selectedGridRows = new List<IBacklogGridRow>();
		private readonly IList<IBacklogGridRow> _allGridRows = new List<IBacklogGridRow>();
		private IBacklogGridRow _expandedRow;

		public BacklogView(IComponentContext container, DateOnlyPeriod period)
		{
			_container = container;
			_period = period;
			InitializeComponent();
			_model = new BacklogModel(container, period);
			
			dateTimePicker1.MinDate = _period.StartDate;
			dateTimePicker1.MaxDate = _period.EndDate.AddDays(1);
			dateTimePicker1.Value = _period.StartDate;
			setupGrid();
		}

		private void setupGrid()
		{
			gridControl1.CellModels.Add("Time", new TimeSpanDurationCellModel(gridControl1.Model) { DisplaySeconds = false, AllowEmptyCell = true });
			gridControl1.CellModels.Add("TimeS", new TimeSpanDurationStaticCellModel(gridControl1.Model) { DisplaySeconds = false });
			gridControl1.CellModels.Add("Percent", new PercentReadOnlyCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("Ignore", new IgnoreCellModel(gridControl1.Model));

			_allGridRows.Add(new BacklogGridHeaderRow(_model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Incoming Demand",
				_model.GetIncomingForIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Planned Time on Incoming",
				_model.GetProductPlanTimeOnIncoming, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Planned Backlog on Incoming",
				_model.GetProductPlanBacklogOnIncoming, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "Time", "Manual Production Plan",
				_model.GetManualEntryOnIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Planned on skill",
				_model.GetForecastedForIndex, _model));

			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled on skill",
				_model.GetScheduledOnIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled backlog on skill",
				_model.GetScheduledBacklogOnIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled on incoming",
				_model.GetScheduledOnIncomingIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Backlog on incoming",
				_model.GetScheduledBacklogOnIncomingIndex, _model));
			_allGridRows.Add(new BacklogGridPercentRow(BacklogCategory.Scheduled, "Percent", "SL on incoming",
				_model.GetScheduledServiceLevelOnIncomingIndex, _model));

			_selectedGridRows = new List<IBacklogGridRow>(_allGridRows);
			_fixedRows = _selectedGridRows.Count-1;

			_expandedRow = new BacklogGridExpandedRow(_model, _fixedRows);
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
				e.Count = _fixedRows + _model.GetIncomingCount((ISkill)tabControlSkills.SelectedTab.Tag);
			else
			{
				e.Count = _fixedRows;
			}
			e.Handled = true;
		}

		private void gridControl1_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			var skill = (ISkill) tabControlSkills.SelectedTab.Tag;
			if (skill == null)
				return;

			if (e.RowIndex == -1)
				return;

			if (e.RowIndex <= _fixedRows)
			{
				_selectedGridRows[e.RowIndex].SetCell(skill, e, new DateOnly(dateTimePicker1.Value));
				if(e.RowIndex > 1 && _selectedGridRows[e.RowIndex].Category != _selectedGridRows[e.RowIndex-1].Category)
					e.Style.Borders.Top = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.ExtraThick);
			}
			else
			{
				_expandedRow.SetCell(skill, e, new DateOnly(dateTimePicker1.Value));
			}

			e.Handled = true;
		}

		//private static void setValue(GridQueryCellInfoEventArgs e, TimeSpan time)
		//{
		//	e.Style.CellValue = time > TimeSpan.Zero ? (object) time : string.Empty;
		//}

		//private void setBackColor(GridQueryCellInfoEventArgs e)
		//{
		//	e.Style.BackColor = Color.White;
		//	if (_model.IsClosedOnIndex(e.ColIndex, (ISkill) tabControlSkills.SelectedTab.Tag))
		//	{
		//		e.Style.BackColor = Color.Khaki;
		//		return;
		//	}
		//	var dateForIndex = _model.GetDateOnIndex(e.ColIndex);
		//	if (e.RowIndex > 0 && e.RowIndex <= plannedRows && dateForIndex < new DateOnly(dateTimePicker1.Value))
		//	{
		//		e.Style.BackColor = Color.LightGray;
		//		return;
		//	}

		//	if (e.RowIndex > plannedRows && dateForIndex >= new DateOnly(dateTimePicker1.Value))
		//	{
		//		e.Style.BackColor = Color.LightGray;
		//	}
		//}

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

		private void toolStripButtonSave_Click(object sender, EventArgs e)
		{
			_model.TransferSkillDays((ISkill)tabControlSkills.SelectedTab.Tag);
		}

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			_model.Load(backgroundWorker1, new DateOnly(dateTimePicker1.Value));
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
			gridControl1.Rows.FreezeRange(1, _fixedRows);
			gridControl1.EndUpdate(true);
			chart1.Palette = ChartColorPalette.SeaGreen;
			updateChartChart();
			toolStripStatusLabel1.Text = string.Empty;
		}

		private void toolStripButtonSkillMapper_Click(object sender, EventArgs e)
		{
			using (var dialog = new SkillMapperDialog(_model.SkillPairsBackofficeEmail))
			{
				dialog.ShowDialog(this);
			}
		}

		private void updateChartChart()
		{
			chart1.Series.Clear();
			chart1.ChartAreas[0].AxisX.Interval = 7;
			chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
			var series1 = chart1.Series.Add("Planned/Scheduled on incoming");
			var series2 = chart1.Series.Add("Backlog on incoming");
			var series3 = chart1.Series.Add("Overstaff");
			series3.Color = Color.OrangeRed;
			series1.ChartType = SeriesChartType.StackedColumn;
			series1.XValueType = ChartValueType.Date;
			series2.ChartType = SeriesChartType.StackedColumn;
			series2.XValueType = ChartValueType.Date;
			series3.ChartType = SeriesChartType.StackedColumn;
			series3.XValueType = ChartValueType.Date;
			for (int i = 1; i < gridControl1.ColCount; i++)
			{
				var dateOnIndex = _model.GetDateOnIndex(i);
				var skill = (ISkill) tabControlSkills.SelectedTab.Tag;
				DataPoint datapoint;
				if(dateOnIndex < dateTimePicker1.Value)
				{
					datapoint = new DataPoint(i, _model.GetScheduledOnIncomingIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					datapoint.AxisLabel = dateOnIndex.ToShortDateString();
					series1.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledBacklogOnIncomingIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series2.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledOverstaffOnIncomming(i, skill).TotalHours);
					series3.Points.Add(datapoint);
				}
				else
				{
					datapoint = new DataPoint(i, _model.GetProductPlanTimeOnIncoming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					datapoint.AxisLabel = dateOnIndex.ToShortDateString();
					series1.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetProductPlanBacklogOnIncoming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series2.Points.Add(datapoint);
				}
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
			_model = new BacklogModel(_container, _period);
			backlogViewLoad(this, new EventArgs());
		}

		private void gridControl1_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.RowIndex != 4)
				return;

			if (e.Style.CellValue == null)
			{
				_model.ClearManualEntryOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag, new DateOnly(dateTimePicker1.Value));
			}
			else
			{
				var value = (TimeSpan)e.Style.CellValue;
				_model.SetManualEntryOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag, value,
					new DateOnly(dateTimePicker1.Value));
			}
			
			updateChartChart();
			gridControl1.Refresh();

			e.Handled = true;
		}

		private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
		{
			if(!_model.Loaded)
				return;

			_model.TransferBacklogs(new DateOnly(dateTimePicker1.Value));
			updateChartChart();
			gridControl1.Refresh();
		}
	}
}
 