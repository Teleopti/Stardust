using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting.Forms.JobHistory
{
    public partial class JobHistoryDetailedView : BaseRibbonForm, IJobHistoryView
    {
        private readonly IJobHistoryProvider _jobHistoryProvider;
        private readonly IDetailedJobHistoryProvider _detailedJobHistoryProvider;
        private JobHistoryPresenter _presenter;
        private DetailedJobHistoryPresenter _detailedJobHistoryPresenter;

        public JobHistoryDetailedView(IJobHistoryProvider jobHistoryProvider, IDetailedJobHistoryProvider detailedJobHistoryProvider)
        {
            _jobHistoryProvider = jobHistoryProvider;
            _detailedJobHistoryProvider = detailedJobHistoryProvider;
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

            _presenter = new JobHistoryPresenter(this, _jobHistoryProvider, new PagingDetail { Take = 20 });
            _presenter.Initialize();
        }

        public void BindData(IEnumerable<JobResultModel> jobResultModels)
        {
            gridControlJobHistory.DataSource = jobResultModels;
        }

        public void BindJobDetailData(IList<DetailedJobHistoryResultModel> jobHistoryEntries)
        {
            gridControlDetailedJobHistory.DataSource = jobHistoryEntries;
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
            _presenter.ReloadHistory();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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
        
        private void linkLabelPreviousLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _presenter.Previous();
        }

        private void linkLabelNextLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _presenter.Next();
        }

        public void LoadJobHistoryData(JobResultModel jobResultModel)
        {
            _detailedJobHistoryPresenter = new DetailedJobHistoryPresenter(this, _detailedJobHistoryProvider);
            _detailedJobHistoryPresenter.LoadDetailedHistory(jobResultModel);
        }

        private void gridControlJobHistoryCellClick(object sender, GridCellClickEventArgs e)
        {
            if (e.RowIndex == 0) return;
            var temp = (IList) gridControlJobHistory.DataSource;
            var jobResult = (JobResultModel) temp[e.RowIndex - 1];
            LoadJobHistoryData(jobResult);
            resizeJobDetailColumns();
        }
    }
}
