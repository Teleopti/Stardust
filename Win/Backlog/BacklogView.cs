using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class BacklogView : Form
	{
		private BacklogModel _model;

		public BacklogView()
		{
			InitializeComponent();
			_model = new BacklogModel();
			
		}

		private void populateTabControl()
		{
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
			e.Count = 5;
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
			_model.Load();
			populateTabControl();
			gridControl1.ControllerOptions = GridControllerOptions.All & (~GridControllerOptions.OleDataSource);
			gridControl1.ResetVolatileData();
			gridControl1.BeginUpdate();
			gridControl1.CellModels.Add("TimeSpanLongHourMinutesStatic", new TimeSpanDurationStaticCellModel(gridControl1.Model) { DisplaySeconds = false });
			//gridControl1.CellModels.Add("Ignore", new IgnoreCellModel(gridControl1.Model));
			gridControl1.ColWidths.SetSize(0, 120);
			gridControl1.RowHeights.SetSize(0, 30);
			
			gridControl1.EndUpdate(true);
		}

		private void tabControlSkills_SelectedIndexChanged(object sender, EventArgs e)
		{
			tabControlSkills.SelectedTab.Controls.Add(splitContainer1);
			splitContainer1.Dock = DockStyle.Fill;
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
	}
}
