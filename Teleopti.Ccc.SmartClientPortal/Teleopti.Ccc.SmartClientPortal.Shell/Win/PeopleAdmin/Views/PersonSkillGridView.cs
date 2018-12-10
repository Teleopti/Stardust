using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	class PersonSkillGridView : GridViewBase
	{
		private List<IColumn<PersonSkillModel>> _gridColumns = new List<IColumn<PersonSkillModel>>();

		private ColumnBase<PersonSkillModel> _checkBoxColumn;
		private ColumnBase<PersonSkillModel> _roleColumn;
		private CheckColumn<PersonSkillModel> _activeCheckBoxColumn;
		private ProficiencyColumn<PersonSkillModel> _profColumn;

		public PersonSkillGridView(GridControl view, FilteredPeopleHolder filteredPeopleHolder) : base(view, filteredPeopleHolder) { }

		internal override ViewType Type => ViewType.SkillsView;
		
		internal override void CreateHeaders()
		{
			_gridColumns.Add(new RowHeaderColumn<PersonSkillModel>());

			_checkBoxColumn = new CheckColumn<PersonSkillModel>("TriState", "1", "0", "2", typeof(int), Resources.IsIn);
			_checkBoxColumn.CellChanged += checkBoxColumnCellChanged;
			_gridColumns.Add(_checkBoxColumn);

			_activeCheckBoxColumn = new CheckColumn<PersonSkillModel>("ActiveTriState", "1", "0", "2", typeof(int), Resources.Active);
			_activeCheckBoxColumn.CellChanged += activeCheckBoxColumnCellChanged;
			_gridColumns.Add(_activeCheckBoxColumn);

			_roleColumn = new ReadOnlyTextColumn<PersonSkillModel>("DescriptionText", Resources.PersonSkill);
			_gridColumns.Add(_roleColumn);

			_profColumn = new ProficiencyColumn<PersonSkillModel>("ProficiencyValues", "Proficiency", Resources.ProficiencyPercent);

			_gridColumns.Add(_profColumn);
			_profColumn.CellChanged += profColumnCellChanged;
		}

		void profColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonSkillModel> e)
		{
			if (FilteredPeopleHolder.SelectedPeoplePeriodGridCollection == null)
			{
				return;
			}

			if (e.DataItem.TriState == 1)
				WorksheetStateHolder.SetPersonSkillForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, new Percent(e.DataItem.Proficiency / 100d));
			// this will fix bug #43259 if want to do it that way, remove the other check of tristate and call then 2 rows above
			//WorksheetStateHolder.SetProfiencyForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, new Percent(e.DataItem.Proficiency / 100d));
		}

		internal override void PrepareView()
		{
			ColCount = _gridColumns.Count;
			RowCount = FilteredPeopleHolder.PersonSkillViewAdapterCollection.Count;

			Grid.ColCount = ColCount - 1;
			Grid.RowCount = RowCount;

			Grid.Cols.HeaderCount = 0;
			Grid.Rows.HeaderCount = 0;

			//Grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			Grid.ColWidths[1] = 40;
			Grid.ColWidths[2] = 60;
			Grid.ColWidths[3] = _gridColumns[3].PreferredWidth + 30;
			Grid.ColWidths[4] = _gridColumns[4].PreferredWidth;
			Grid.Name = "PersonalSkillView";
			HideRowHeaderColumn();
			Grid.ClipboardPaste += gridWorksheet_ClipboardPaste;
		}

		public override void Invalidate()
		{
			Grid.Invalidate();
		}

		public override IEnumerable<Tuple<IPerson, int>> Sort(bool isAscending)
		{
			return Enumerable.Empty<Tuple<IPerson, int>>();
		}

		public override void PerformSort(IEnumerable<Tuple<IPerson, int>> order)
		{
		}

		private void gridWorksheet_ClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
			ClipboardPaste(e);
			e.Handled = true;
		}

		private void activeCheckBoxColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonSkillModel> e)
		{
			if (FilteredPeopleHolder.SelectedPeoplePeriodGridCollection == null)
			{
				e.DataItem.ActiveTriState = 0; return;
			}

			if (e.DataItem.ActiveTriState == 2) e.DataItem.ActiveTriState = 1;

			if (e.DataItem.ActiveTriState == 1)
			{
				WorksheetStateHolder.SetPersonSkillForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, new Percent(e.DataItem.Proficiency / 100d));
				WorksheetStateHolder.ChangePersonSkillActiveState(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, true);
				e.DataItem.TriState = e.DataItem.ActiveTriState;
			}
			else
			{
				WorksheetStateHolder.ChangePersonSkillActiveState(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, false);
			}
		}

		private void checkBoxColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonSkillModel> e)
		{
			if (FilteredPeopleHolder.SelectedPeoplePeriodGridCollection == null)
			{
				e.DataItem.TriState = 0; return;
			}

			if (e.DataItem.TriState == 2) e.DataItem.TriState = 1;

			if (e.DataItem.TriState == 1)
			{
				WorksheetStateHolder.SetPersonSkillForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, new Percent(e.DataItem.Proficiency / 100d));
				WorksheetStateHolder.ChangePersonSkillActiveState(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, true);
				e.DataItem.ActiveTriState = e.DataItem.TriState;

			}
			else
			{
				WorksheetStateHolder.RemovePersonSkillForSelectedPersonPeriods(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill);
				WorksheetStateHolder.ChangePersonSkillActiveState(FilteredPeopleHolder.SelectedPeoplePeriodGridCollection, e.DataItem.PersonSkill, false);
				e.DataItem.ActiveTriState = e.DataItem.TriState;
			}
		}

		internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].GetCellInfo(e, FilteredPeopleHolder.PersonSkillViewAdapterCollection);
			}
			base.QueryCellInfo(e);
		}

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].SaveCellInfo(e, FilteredPeopleHolder.PersonSkillViewAdapterCollection);
			}
		}
		protected override void Dispose(bool disposing)
		{
			_activeCheckBoxColumn.CellChanged -= activeCheckBoxColumnCellChanged;
			_checkBoxColumn.CellChanged -= checkBoxColumnCellChanged;
			_profColumn.CellChanged -= profColumnCellChanged;
			Grid.ClipboardPaste -= gridWorksheet_ClipboardPaste;
			_activeCheckBoxColumn = null;
			_roleColumn = null;
			_checkBoxColumn = null;
			_gridColumns = null;
			_profColumn = null;
			base.Dispose(disposing);
		}
	}
}