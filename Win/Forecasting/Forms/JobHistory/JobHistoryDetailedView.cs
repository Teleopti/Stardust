using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting.Forms.JobHistory
{
	public partial class JobHistoryDetailedView : BaseRibbonForm, IJobHistoryView
	{
		public JobHistoryDetailedView()
		{
			InitializeComponent();
				if (!DesignMode)
					SetTexts();
		}

		private void setColors()
        {
            gridControlJobHistory.Properties.BackgroundColor = Color.White;
			gridControlDetailedJobHistory.Properties.BackgroundColor = Color.White;
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DesignMode) return;
			initializeGrid();
			initializeJobHistoryDetailGrid();
			setColors();
			Presenter.Initialize();
			resizeColumns();
		}

		internal JobHistoryPresenter Presenter { get; set; }

		public void BindJobResultData(IEnumerable<JobResultModel> jobResultModels)
		{
			gridControlJobHistory.DataSource = jobResultModels;
			loadDetails();
		}

		public void BindJobResultDetailData(IList<JobResultDetailModel> jobHistoryEntries)
		{
			gridControlDetailedJobHistory.DataSource = jobHistoryEntries;
			resizeJobDetailColumns();
		}

		public void TogglePrevious(bool enabled)
		{
			linkLabelPrevious.Enabled = enabled;
		}

		public void ToggleNext(bool enabled)
		{
			linkLabelNext.Enabled = enabled;
		}

		public void SetResultDescription(string description)
		{
			autoLabelPageCount.Text = description;
		}

		public int DetailLevel { get; private set; }

		private void toolStripButtonReloadHistory_Click(object sender, EventArgs e)
		{
			Presenter.ReloadHistory();
			loadDetails();
		}

		private void resizeColumns()
		{
			var colWidth = (gridControlJobHistory.Width - 12) / gridControlJobHistory.Model.ColCount - 1;
			for (var i = 1; i < gridControlJobHistory.Model.ColCount + 1; i++)
			{
				gridControlJobHistory.Model.ColWidths.SetSize(i, colWidth);
			}
			gridControlJobHistory.Model.ColWidths.SetSize(0, 0);
			for (int i = 0; i <= gridControlJobHistory.Model.RowCount; i++)
			{
				gridControlJobHistory.Model.RowHeights.ResizeToFit(GridRangeInfo.Row(i));
			}
		}

		private void resizeJobDetailColumns()
		{
			gridControlDetailedJobHistory.Model.ColWidths.ResizeToFit(GridRangeInfo.Col(1));
			gridControlDetailedJobHistory.Model.ColWidths.ResizeToFit(GridRangeInfo.Col(2));
			for (int i = 0; i <= gridControlDetailedJobHistory.Model.RowCount; i++)
			{
				gridControlDetailedJobHistory.Model.RowHeights.ResizeToFit(GridRangeInfo.Row(i));
			}
		}

		private void initializeGrid()
		{
			var gridBoundColumnJobCategory = new GridBoundColumn();
			var gridBoundColumnOwner = new GridBoundColumn();
			var gridBoundColumnTimestamp = new GridBoundColumn();
			var gridBoundColumnStatus = new GridBoundColumn();

			gridBoundColumnJobCategory.HeaderText = UserTexts.Resources.JobCategory;
			gridBoundColumnJobCategory.MappingName = "JobCategory";
			gridBoundColumnJobCategory.ReadOnly = true;
			gridBoundColumnJobCategory.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridBoundColumnOwner.HeaderText = UserTexts.Resources.Owner;
			gridBoundColumnOwner.MappingName = "Owner";
			gridBoundColumnOwner.ReadOnly = true;
			gridBoundColumnOwner.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridBoundColumnTimestamp.HeaderText = UserTexts.Resources.StartTime;
			gridBoundColumnTimestamp.MappingName = "Timestamp";
			gridBoundColumnTimestamp.ReadOnly = true;
			gridBoundColumnTimestamp.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridBoundColumnStatus.HeaderText = UserTexts.Resources.Status;
			gridBoundColumnStatus.MappingName = "Status";
			gridBoundColumnStatus.ReadOnly = true;
			gridBoundColumnStatus.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridControlJobHistory.GridBoundColumns.AddRange(new[] {
				gridBoundColumnJobCategory,
				gridBoundColumnOwner,
				gridBoundColumnTimestamp,
				gridBoundColumnStatus});		
		}

		private void initializeJobHistoryDetailGrid()
		{
			var gridBoundColumnTimestamp = new GridBoundColumn();
			var gridBoundColumnMessage = new GridBoundColumn();
			
			gridBoundColumnTimestamp.HeaderText = UserTexts.Resources.Timestamp;
			gridBoundColumnTimestamp.MappingName = "Timestamp";
			gridBoundColumnTimestamp.ReadOnly = true;
			gridBoundColumnTimestamp.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridBoundColumnMessage.HeaderText = UserTexts.Resources.Message;
			gridBoundColumnMessage.MappingName = "Message";
			gridBoundColumnMessage.ReadOnly = true;
			gridBoundColumnMessage.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

			gridControlDetailedJobHistory.GridBoundColumns.AddRange(new[] {
				gridBoundColumnTimestamp,
				gridBoundColumnMessage});
		}

	  
		private void linkLabelPrevious_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Presenter.Previous();
		}

		private void linkLabelNext_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Presenter.Next();
		}

		private void gridControlJobHistory_CellClick(object sender, GridCellClickEventArgs e)
		{
			loadDetails();
		}

		private void ribbonControlAdv1BeforeContextMenuOpen(object sender, Syncfusion.Windows.Forms.Tools.ContextMenuEventArgs e)
		{
			e.Cancel = true;
		}

		private void gridControlJobHistoryPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			e.Style.BackColor = e.RowIndex == gridControlJobHistory.CurrentCell.RowIndex ? Color.LightGoldenrodYellow : Color.White;
		}

		private void loadDetails()
		{
			var row = gridControlJobHistory.CurrentCell.RowIndex;
			if (row > 0)
			{
				var data = (IList)gridControlJobHistory.DataSource;
				var jobResult = (JobResultModel)data[row - 1];
				Presenter.LoadDetailedHistory(jobResult);
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Convert.ToInt32(System.Object)")]
		private void toolStripButtonDetailLevelClick(object sender, EventArgs e)
		{
			toolStripButtonInfo.Checked = false;
			toolStripButtonWarnings.Checked = false;
			toolStripButtonErrors.Checked = false;
			var butt = (ToolStripButton) sender;
			{
				butt.Checked = true;
				DetailLevel = Convert.ToInt32(butt.Tag);
			}
		
			loadDetails();
		}

		private void ribbonControlAdv1_Click(object sender, EventArgs e)
		{

		}
	}
}
