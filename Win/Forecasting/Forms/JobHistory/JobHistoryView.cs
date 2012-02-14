using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting.Forms.JobHistory
{
    public partial class JobHistoryView : BaseRibbonForm, IJobHistoryView
    {
        private JobHistoryPresenter _presenter;

        public JobHistoryView()
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
            gridControlJobHistory.Properties.BackgroundColor = ColorHelper.WizardPanelBackgroundColor();
            BackColor = ColorHelper.WizardBackgroundColor();
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DesignMode) return;

			_presenter = new JobHistoryPresenter(this, new JobHistoryProvider(UnitOfWorkFactory.Current,new JobResultRepository(UnitOfWorkFactory.Current)),new PagingDetail{Take = 20});
			_presenter.Initialize();
		}

    	public void BindData(IEnumerable<JobResultModel> jobResultModels)
    	{
			gridControlJobHistory.DataSource = jobResultModels;
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
            //Argh syncfusion
            var colWidth = (gridControlJobHistory.Width - 12) / gridControlJobHistory.Model.ColCount - 1;
            for (var i = 1; i < gridControlJobHistory.Model.ColCount + 1; i++)
            {
                gridControlJobHistory.Model.ColWidths.SetSize(i, colWidth);
            }
            gridControlJobHistory.Model.ColWidths.SetSize(0, 0);
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

        private void buttonAdvClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void JobHistoryView_SizeChanged(object sender, EventArgs e)
        {
            resizeColumns();
        }

        private void JobHistoryView_LocationChanged(object sender, EventArgs e)
        {
            resizeColumns();
            gridControlJobHistory.Refresh();
        }

		private void linkLabelPrevious_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			_presenter.Previous();
		}

		private void linkLabelNext_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			_presenter.Next();
		}
    }
}
