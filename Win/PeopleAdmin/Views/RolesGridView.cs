﻿using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    public class RolesGridView : GridViewBase
    {
        private readonly List<IColumn<RolesModel>> _gridColumns = new List<IColumn<RolesModel>>();

        private ColumnBase<RolesModel> _checkBoxColumn;
        private ColumnBase<RolesModel> _roleColumn;

        public RolesGridView(GridControl view, FilteredPeopleHolder filteredPeopleHolder) : base(view, filteredPeopleHolder) { }

        internal override ViewType Type
        {
            get { return ViewType.RolesView; }
        }

        internal override void CreateHeaders()
        {
            _gridColumns.Add(new RowHeaderColumn<RolesModel>());

            _checkBoxColumn = new CheckColumn<RolesModel>("TriState", "1", "0", "2", typeof(int), UserTexts.Resources.IsIn);
            _checkBoxColumn.CellChanged += checkBoxColumn_CellChanged;
            _gridColumns.Add(_checkBoxColumn);

            _roleColumn = new ReadOnlyTextColumn<RolesModel>("Role.DescriptionText", UserTexts.Resources.Roles);
            _gridColumns.Add(_roleColumn);
        }
        internal override void PrepareView()
        {
            ColCount = _gridColumns.Count;
            RowCount = FilteredPeopleHolder.RolesViewAdapterCollection.Count;

            Grid.ColCount = ColCount - 1;
            Grid.RowCount = RowCount;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;

            Grid.ColWidths[1] = 40;
            Grid.ColWidths[2] = _gridColumns[2].PreferredWidth + 30;
            Grid.Name = "RolesView";
            HideRowHeaderColumn();
        }

        public override void Invalidate()
        {
            Grid.Invalidate();
        }

        private void checkBoxColumn_CellChanged(object sender, ColumnCellChangedEventArgs<RolesModel> e)
        {
            if (FilteredPeopleHolder.SelectedPeopleGeneralGridData == null)
            {
                e.DataItem.TriState = 0; return;
            }
            if (e.DataItem.TriState == 2) e.DataItem.TriState = 1;

            if (e.DataItem.TriState == 1)
                FilteredPeopleHolder.SetRoleForSelectedPersons(e.DataItem.Role);
            else
                FilteredPeopleHolder.RemoveRoleForSelectedPersons(e.DataItem.Role);
        }

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                _gridColumns[e.ColIndex].GetCellInfo(e, FilteredPeopleHolder.RolesViewAdapterCollection);
            }

			base.QueryCellInfo(e);
        }
 
        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                _gridColumns[e.ColIndex].SaveCellInfo(e, FilteredPeopleHolder.RolesViewAdapterCollection);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_checkBoxColumn!=null)
            {
                _checkBoxColumn.CellChanged -= checkBoxColumn_CellChanged;
            }
        }
    }
}
