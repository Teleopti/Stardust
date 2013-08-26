using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftPerAgentGrid : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext, IShiftPerAgentGrid
    {
	    private readonly IDistributionInformationExtractor _model;
	    private ShiftPerAgentGridPresenter _presenter;
	    private readonly ISchedulerStateHolder _schedulerState;

		public ShiftPerAgentGrid(IDistributionInformationExtractor model, ISchedulerStateHolder schedulerState)
		{
			_model = model;
			_schedulerState = schedulerState;
			_presenter = new ShiftPerAgentGridPresenter(this);

			initializeComponent();
		}

		private void initializeComponent()
		{
			//ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);

			ResetVolatileData();

			QueryColCount += shiftPerAgentGridQueryColCount;
			QueryRowCount += shiftPerAgentGridQueryRowCount;
			QueryCellInfo += shiftPerAgentGridQueryCellInfo;

			//ColWidths.ResizeToFit(GridRangeInfo.Table());

			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			ResumeLayout(false);	
		}

	    void shiftPerAgentGridQueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex == 0 && e.RowIndex == 0) return;
			if (e.ColIndex > _model.ShiftCategories.Count) return;
			if (e.RowIndex > _model.PersonInvolved.Count) return;

			if (e.ColIndex > 0 && e.RowIndex == 0)
			{
				e.Style.CellValue = _model.ShiftCategories[e.ColIndex - 1];
				e.Style.Tag = _model.ShiftCategories[e.ColIndex - 1];
			}

			if (e.ColIndex == 0 && e.RowIndex > 0)
			{
				e.Style.CellValue = _schedulerState.CommonAgentName(_model.PersonInvolved[e.RowIndex - 1]);
				e.Style.Tag = _model.PersonInvolved[e.RowIndex - 1];
			}

			if (e.ColIndex > 0 && e.RowIndex > 0)
			{
				var person = this[e.RowIndex, 0].Tag as IPerson;
				var shiftCategory = this[0, e.ColIndex].Tag as string;

				e.Style.CellValue = 0;

				foreach (var shiftCategoryPerAgent in _model.GetShiftCategoryPerAgent())
				{
					if (shiftCategoryPerAgent.Person.Equals(person))
					{
						if (shiftCategoryPerAgent.ShiftCategoryName.Equals(shiftCategory))
						{
							e.Style.CellValue = shiftCategoryPerAgent.Count;
							break;
						}
					}
				}
			}

			e.Handled = true;
		}

		void shiftPerAgentGridQueryRowCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
		{
			e.Count = _model.PersonInvolved.Count;
			e.Handled = true;
		}

		void shiftPerAgentGridQueryColCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
		{
			e.Count = _model.ShiftCategories.Count;
			e.Handled = true;
		}

        public bool HasColumns { get; private set; }
        public void RefreshGrid()
        {
            throw new NotImplementedException();
        }

        public AbstractDetailView Owner { get; set; }
        public void GoToDate(DateTime theDate)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IDictionary<int, GridRow> EnabledChartGridRows { get; private set; }
        public ReadOnlyCollection<GridRow> AllGridRows { get; private set; }
        public int MainHeaderRow { get; private set; }
        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            throw new NotImplementedException();
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            throw new NotImplementedException();
        }

        public bool HasHelp { get; private set; }
        public string HelpId { get; private set; }
    }
}
