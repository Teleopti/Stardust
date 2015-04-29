using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autofac;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
			
			dateTimePicker1.MinDate = _period.StartDate.Date;
			dateTimePicker1.MaxDate = _period.EndDate.AddDays(1).Date;
			dateTimePicker1.Value = _period.StartDate.Date;
			setupGrid();
		}

		private void setupGrid()
		{
			gridControl1.CellModels.Add("Time", new TimeSpanDurationCellModel(gridControl1.Model) { DisplaySeconds = false, AllowEmptyCell = true });
			gridControl1.CellModels.Add("TimeS", new TimeSpanDurationStaticCellModel(gridControl1.Model) { DisplaySeconds = false });
			gridControl1.CellModels.Add("Percent", new PercentReadOnlyCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("Ignore", new IgnoreCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("NumericS", new NumericReadOnlyCellModel(gridControl1.Model){NumberOfDecimals = 0});

			_allGridRows.Add(new BacklogGridHeaderRow(_model));
			setupIncoming();
			setupProductionPlan();
			setupScheduled();

			_selectedGridRows = new List<IBacklogGridRow>();
			foreach (var backlogGridRow in _allGridRows)
			{
				//if(backlogGridRow.Category == BacklogCategory.Scheduled && !toolStripButtonShowScheduled.Checked)
				//	continue;
				_selectedGridRows.Add(backlogGridRow);
			}
			_fixedRows = _selectedGridRows.Count-1;

			_expandedRow = new BacklogGridExpandedRow("TimeS", _model, _fixedRows);
		}

		private void setupScheduled()
		{
			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.Scheduled, "NumericS", "Scheduled production",
				_model.GetScheduledWorkOnIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled time",
				_model.GetScheduledTimeOnIndex, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.Scheduled, "NumericS", "Estimated total backlog",
				_model.GetScheduledBacklogWorkOnIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Estimated total backlog time",
				_model.GetScheduledBacklogTimeOnIndex, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.Scheduled, "NumericS", "Scheduled production on incoming",
				_model.GetScheduledWorkOnIncomingIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled time on incoming",
				_model.GetScheduledTimeOnIncomingIndex, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.Scheduled, "NumericS",
				"Remaining work after SLA on incoming",
				_model.GetScheduledBacklogWorkOnIncomingIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Remaining time after SLA on incoming",
				_model.GetScheduledBacklogTimeOnIncomingIndex, _model));

			_allGridRows.Add(new BacklogGridPercentRow(BacklogCategory.Scheduled, "Percent", "Estimated Service Level on incoming",
				_model.GetScheduledServiceLevelOnIncomingIndex, _model));

			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Scheduled, "TimeS", "Scheduled overstaffing time",
				_model.GetScheduledOverstaffOnIncomming, _model));
		}

		private void setupProductionPlan()
		{
			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.ProductionPlan, "NumericS", "Work on incoming",
				_model.GetProductPlanWorkOnIncoming, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Time on incoming",
				_model.GetProductPlanTimeOnIncoming, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.ProductionPlan, "NumericS", "Remaining work after SLA on incoming",
				_model.GetProductPlanBacklogWorkOnIncoming, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Remaining time after SLA on incoming",
				_model.GetProductPlanBacklogTimeOnIncoming, _model));

			_allGridRows.Add(new BacklogEditableGridRow(BacklogCategory.ProductionPlan, "Time", "Manual production time",
				_model.GetManualEntryOnIndex, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.ProductionPlan, "NumericS", "Planned production",
				_model.GetForecastedWorkForIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Planned time",
				_model.GetForecastedForIndex, _model));

			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.ProductionPlan, "NumericS", "Estimated total backlog",
				_model.GetAccumulatedPlannedBacklogWorkForIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Estimated total backlog",
				_model.GetAccumulatedPlannedBacklogTimeForIndex, _model));

			_allGridRows.Add(new BacklogGridRow(BacklogCategory.ProductionPlan, "TimeS", "Planned overstaffing time",
				_model.GetPlannedOverstaffingTimeForIndex, _model));
		}

		private void setupIncoming()
		{
			_allGridRows.Add(new BacklogGridWorkRow(BacklogCategory.Incoming, "NumericS", "Incoming work",
				_model.GetIncomingWorkForIndex, _model));
			_allGridRows.Add(new BacklogGridRow(BacklogCategory.Incoming, "TimeS", "Incoming time",
				_model.GetIncomingTimeForIndex, _model));
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
			if (!_model.Loaded)
				return;

			var campaign = new Campaign { Name = "testCampaign2" };
			var campaignWorkingPeriod = new CampaignWorkingPeriod();
			campaignWorkingPeriod.TimePeriod = new TimePeriod(10, 0, 15, 0);

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);
			var outboundSkillCreator = _container.Resolve<OutboundSkillCreator>();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var skill = outboundSkillCreator.CreateSkill(_model.GetAllActivities().First(), campaign);
				var skillrepository = new SkillRepository(uow);
				skillrepository.Add(skill);
				var workloadRepository = new WorkloadRepository(uow);
				workloadRepository.Add(skill.WorkloadCollection.First());
				uow.PersistAll();
			}

			populateTabControl();
			gridControl1.ControllerOptions = GridControllerOptions.All & (~GridControllerOptions.OleDataSource);
			gridControl1.ResetVolatileData();
			gridControl1.BeginUpdate();
			gridControl1.ColWidths.SetSize(0, 179);
			gridControl1.RowHeights.SetSize(0, 27);
			gridControl1.Rows.FreezeRange(1, _fixedRows);
			gridControl1.EndUpdate(true);
			chart1.Palette = ChartColorPalette.SeaGreen;
			updateChartChart();
			toolStripStatusLabel1.Text = string.Empty;
		}

		private void toolStripButtonSkillMapper_Click(object sender, EventArgs e)
		{
			using (var dialog = new SkillMapperDialog(_model.SkillMappings, _model.GetAllSkills()))
			{
				dialog.ShowDialog(this);
				if(dialog.DialogResult == DialogResult.OK)
				{
					_model.SkillMappings.Clear();
					foreach (var skillMapDev in dialog.Mappings())
					{
						_model.SkillMappings.Add(skillMapDev);
					}
					_model.SaveMappings();
				}
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
			var series4 = chart1.Series.Add("Estimated total backlog");
			var series5 = chart1.Series.Add("Planned time");

			series1.ChartType = SeriesChartType.StackedColumn;
			series1.XValueType = ChartValueType.Date;
			series1.Color = Color.LightGreen;
			series2.ChartType = SeriesChartType.StackedColumn;
			series2.XValueType = ChartValueType.Date;
			series2.Color = Color.Yellow;
			series3.ChartType = SeriesChartType.StackedColumn;
			series3.XValueType = ChartValueType.Date;
			series3.Color = Color.LightCoral;
			series4.ChartType = SeriesChartType.Line;
			series4.YAxisType = AxisType.Primary;
			series4.XValueType = ChartValueType.Date;
			series4.Color = Color.Red;
			series5.ChartType = SeriesChartType.Line;
			series5.YAxisType = AxisType.Primary;
			series5.XValueType = ChartValueType.Date;
			series5.Color = Color.Blue;

			for (int i = 1; i < gridControl1.ColCount; i++)
			{
				var dateOnIndex = _model.GetDateOnIndex(i).Date;
				var skill = (ISkill) tabControlSkills.SelectedTab.Tag;
				DataPoint datapoint;
				if(dateOnIndex < dateTimePicker1.Value)
				{
					datapoint = new DataPoint(i, _model.GetScheduledTimeOnIncomingIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					datapoint.AxisLabel = dateOnIndex.ToShortDateString();
					series1.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledBacklogTimeOnIncomingIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series2.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledOverstaffOnIncomming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series3.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledBacklogTimeOnIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series4.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetScheduledTimeOnIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series5.Points.Add(datapoint);
					//_model.GetScheduledTimeOnIndex
					
				}
				else
				{
					datapoint = new DataPoint(i, _model.GetProductPlanTimeOnIncoming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					datapoint.AxisLabel = dateOnIndex.ToShortDateString();
					series1.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetProductPlanBacklogTimeOnIncoming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series2.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetPlannedOverstaffingTimeForIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series3.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetAccumulatedPlannedBacklogTimeForIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series4.Points.Add(datapoint);
					datapoint = new DataPoint(i, _model.GetForecastedForIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
					series5.Points.Add(datapoint);
					//_model.GetForecastedForIndex
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
			

			//_model = new BacklogModel(_container, _period);
			//backlogViewLoad(this, new EventArgs());
		}

		private void gridControl1_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			var row = _selectedGridRows[e.RowIndex] as BacklogEditableGridRow;
			if (row == null)
				return;

			if (e.Style.CellValue == null)
			{
				_model.ClearManualEntryOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag, new DateOnly(dateTimePicker1.Value));
			}
			else
			{
				var value = (TimeSpan)e.Style.CellValue;
				_model.SetManualEntryOnIndex(e.ColIndex, (ISkill)tabControlSkills.SelectedTab.Tag, value);
			}
			
			updateChartChart();
			gridControl1.Refresh();

			e.Handled = true;
		}

		private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
		{
			if(!_model.Loaded)
				return;

			_model.SetProductPlanStart(new DateOnly(dateTimePicker1.Value));
			_model.TransferBacklogs();
			updateChartChart();
			gridControl1.Refresh();
		}

		private void toolStripButtonShowScheduled_CheckedChanged(object sender, EventArgs e)
		{
			_selectedGridRows.Clear();
			foreach (var backlogGridRow in _allGridRows)
			{
				if (backlogGridRow.Category == BacklogCategory.Scheduled && !toolStripButtonShowScheduled.Checked)
					continue;
				_selectedGridRows.Add(backlogGridRow);
			}
			_fixedRows = _selectedGridRows.Count - 1;
			gridControl1.ResetVolatileData();
			gridControl1.Refresh();
		}
	}
}
 