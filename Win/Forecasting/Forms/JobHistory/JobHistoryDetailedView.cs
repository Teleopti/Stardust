using System;
using System.Collections;
using System.Collections.Generic;
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
            initializeGrid();
            initializeJobHistoryDetailGrid();
            resizeColumns();
            setColors();
            if (!DesignMode)
                SetTexts();
        }

        private void setColors()
        {
            gridControlJobHistory.Properties.BackgroundColor = ColorHelper.WizardPanelBackgroundColor();
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;

            Presenter.Initialize();
        }

        internal JobHistoryPresenter Presenter { get; set; }

        public void BindJobResultData(IEnumerable<JobResultModel> jobResultModels)
        {
            gridControlJobHistory.DataSource = jobResultModels;
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

        private void toolStripButtonReloadHistory_Click(object sender, EventArgs e)
        {
            Presenter.ReloadHistory();
        }

        private void resizeColumns()
        {
            var colWidth = (gridControlJobHistory.Width - 12) / gridControlJobHistory.Model.ColCount - 1;
            for (var i = 1; i < gridControlJobHistory.Model.ColCount + 1; i++)
            {
                gridControlJobHistory.Model.ColWidths.SetSize(i, colWidth);
            }
            gridControlJobHistory.Model.ColWidths.SetSize(0, 0);
        }

        private void resizeJobDetailColumns()
        {
            gridControlDetailedJobHistory.Model.ColWidths.ResizeToFit(GridRangeInfo.Col(1));
            gridControlDetailedJobHistory.Model.ColWidths.ResizeToFit(GridRangeInfo.Col(2));
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

            gridBoundColumnTimestamp.HeaderText = UserTexts.Resources.Timestamp;
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
            if (e.RowIndex != 0)
            {
                var data = (IList) gridControlJobHistory.DataSource;
                var jobResult = (JobResultModel) data[e.RowIndex - 1];
                Presenter.LoadDetailedHistory(jobResult);
            }
        }
    }
}
