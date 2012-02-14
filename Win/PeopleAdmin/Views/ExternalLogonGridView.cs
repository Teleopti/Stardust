using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    public class ExternalLogOnGridView:GridViewBase
    {
        private readonly List<IColumn<ExternalLogOnModel>> _gridColumns = new List<IColumn<ExternalLogOnModel>>();

		private ColumnBase<ExternalLogOnModel> _checkBoxColumn;
		private ColumnBase<ExternalLogOnModel> _externalLogonColumn;
		private ColumnBase<ExternalLogOnModel> _acdObjectColumn;

        internal override ViewType Type
        {
            get { return ViewType.ExternalLogOnView; }
        }

        public ExternalLogOnGridView(GridControl view, FilteredPeopleHolder filteredPeopleHolder) : base(view, filteredPeopleHolder)
        {
        	if (filteredPeopleHolder == null) throw new ArgumentNullException("filteredPeopleHolder");
        	filteredPeopleHolder.SetFilteredExternalLogOnCollection(filteredPeopleHolder.ExternalLogOnViewAdapterCollection);
        }

    	internal override void CreateHeaders()
        {
            _gridColumns.Add(new RowHeaderColumn<ExternalLogOnModel>());

            _checkBoxColumn = new CheckColumn<ExternalLogOnModel>("TriState", "1", "0", "2", typeof(int), Resources.IsIn);
            _checkBoxColumn.CellChanged += checkBoxColumn_CellChanged;
            _gridColumns.Add(_checkBoxColumn);

            _externalLogonColumn= new ReadOnlyTextColumn<ExternalLogOnModel>("DescriptionText", Resources.ExternalLogOn);
            _gridColumns.Add(_externalLogonColumn);

			_acdObjectColumn = new ReadOnlyTextColumn<ExternalLogOnModel>("AcdText", Resources.LogObject);
			_gridColumns.Add(_acdObjectColumn);
        }

        internal override void PrepareView()
        {
            ColCount = _gridColumns.Count;
           
			RowCount = FilteredPeopleHolder.FilteredExternalLogOnCollection.Count;

            Grid.ColCount = ColCount - 1;
            Grid.RowCount = RowCount;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;

            Grid.ColWidths[1] = 40;
            Grid.ColWidths[2] = _gridColumns[2].PreferredWidth + 30;
            Grid.Name = "ExternalLogOnView";

            HideRowHeaderColumn();
        }

        public override void Invalidate()
        {
            Grid.Invalidate();
        }

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
              _gridColumns[e.ColIndex].GetCellInfo(e, FilteredPeopleHolder.FilteredExternalLogOnCollectionCellStages);
            }
			base.QueryCellInfo(e);
        }
       
        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
				_gridColumns[e.ColIndex].SaveCellInfo(e, FilteredPeopleHolder.FilteredExternalLogOnCollectionCellStages);
            }
        }

        private void checkBoxColumn_CellChanged(object sender, ColumnCellChangedEventArgs<ExternalLogOnModel> e)
        {
            if (Grid.ReadOnly) 
            {
                return;

            }
            if (FilteredPeopleHolder.SelectedPeoplePeriodGridCollection == null)
            {
                e.DataItem.TriState = 0; return;
            }
           
            if (e.DataItem.TriState == 2) e.DataItem.TriState = 1;

            if (e.DataItem.TriState == 1)
				WorksheetStateHolder.SetExternalLogOnForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.ExternalLogOn);
            else
                WorksheetStateHolder.RemoveExternalLogOnForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.ExternalLogOn);
        }
    }
}
