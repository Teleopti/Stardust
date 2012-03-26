using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting.Forms.JobHistory
{
    public partial class DetailedJobHistoryView : BaseRibbonForm, IDetailedJobHistoryView
    {
        private Guid? _jobId;
        private String _jobType;
        
        public DetailedJobHistoryView()
        {
            InitializeComponent();
            initializeGrid();
            resizeColumns();
            setColors();
            
            if (!DesignMode)
                SetTexts();
        }

        private void setColors()
        {
            gridControlDetailedJobHistory.Properties.BackgroundColor = ColorHelper.WizardPanelBackgroundColor();
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        private void initializeGrid()
        {
            var gridBoundColumnTimestamp = new GridBoundColumn();
            var gridBoundColumnMessage = new GridBoundColumn();
            var gridBoundColumnExceptionTrace = new GridBoundColumn();
            var gridBoundColumnExceptionMessage = new GridBoundColumn();
            var gridBoundColumnInnerExceptionTrace = new GridBoundColumn();
            var gridBoundColumnInnerExceptionMessage = new GridBoundColumn();

            gridBoundColumnTimestamp.HeaderText = "TimeStamp";
            gridBoundColumnTimestamp.MappingName = "TimeStamp";
            gridBoundColumnTimestamp.ReadOnly = true;
            gridBoundColumnTimestamp.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

            gridBoundColumnMessage.HeaderText = "Message";
            gridBoundColumnMessage.MappingName = "Message";
            gridBoundColumnMessage.ReadOnly = true;
            gridBoundColumnMessage.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

            gridBoundColumnExceptionTrace.HeaderText = "ExceptionStackTrace";
            gridBoundColumnExceptionTrace.MappingName = "ExceptionStackTrace";
            gridBoundColumnExceptionTrace.ReadOnly = true;
            gridBoundColumnExceptionTrace.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

            gridBoundColumnExceptionMessage.HeaderText = "ExceptionMessage";
            gridBoundColumnExceptionMessage.MappingName = "ExceptionMessage";
            gridBoundColumnExceptionMessage.ReadOnly = true;
            gridBoundColumnExceptionMessage.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;


            gridBoundColumnInnerExceptionTrace.HeaderText = "InnerExceptionStackTrace";
            gridBoundColumnInnerExceptionTrace.MappingName = "InnerExceptionStackTrace";
            gridBoundColumnInnerExceptionTrace.ReadOnly = true;
            gridBoundColumnInnerExceptionTrace.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;

            gridBoundColumnInnerExceptionMessage.HeaderText = "InnerExceptionMessage";
            gridBoundColumnInnerExceptionMessage.MappingName = "InnerExceptionMessage";
            gridBoundColumnInnerExceptionMessage.ReadOnly = true;
            gridBoundColumnInnerExceptionMessage.StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Left;


            gridControlDetailedJobHistory.GridBoundColumns.AddRange(new[] {
                gridBoundColumnTimestamp,
                gridBoundColumnMessage,
                gridBoundColumnExceptionTrace,
                gridBoundColumnExceptionMessage,
                gridBoundColumnInnerExceptionTrace,
                gridBoundColumnInnerExceptionMessage});
        }

        private void resizeColumns()
        {
            //Argh syncfusion
            var colWidth = (gridControlDetailedJobHistory.Width - 12) / gridControlDetailedJobHistory.Model.ColCount - 1;
            for (var i = 1; i < gridControlDetailedJobHistory.Model.ColCount + 1; i++)
            {
                gridControlDetailedJobHistory.Model.ColWidths.SetSize(i, colWidth);
            }
            gridControlDetailedJobHistory.Model.ColWidths.SetSize(0, 0);
        }

        public Guid? JobId
        {
            get { return _jobId; }
            set { _jobId = value; }
        }

        public string JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }

        public void BindData(IList<DetailedJobHistoryResultModel> jobHistoryEntries)
        {
            gridControlDetailedJobHistory.DataSource = jobHistoryEntries;
        }

        public void LoadJobHistoryData(JobResultModel jobResultModel)
        {
            //_presenter = new DetailedJobHistoryPresenter(this, new DetailedJobHistoryProvider(UnitOfWorkFactory.Current, new JobResultRepository(UnitOfWorkFactory.Current)));
            //_presenter.LoadDetailedHistory(jobResultModel);
        }
    }

}
