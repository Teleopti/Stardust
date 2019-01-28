using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public partial class ShiftCategoryLimitationView : BaseUserControl, IShiftCategoryLimitationView
	{
		private ShiftCategoryLimitationViewPresenter _presenter;
		private IList<IShiftCategory> _shiftCategories = new List<IShiftCategory>();
		private FilteredPeopleHolder _filteredPeopleHolder;
		private GridConstructor _gridConstructor;
		private GridViewBase _gridViewBase;

		public ShiftCategoryLimitationView()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				_presenter = new ShiftCategoryLimitationViewPresenter(this);
				SetTexts();
			}
		}

		public void SetupGrid()
		{
			NullableIntegerCellModel nullableIntegerCell = new NullableIntegerCellModel(gridLimitatation.Model);
			nullableIntegerCell.OnlyPositiveValues = true;
			if (!gridLimitatation.CellModels.ContainsKey("NullableInteger"))
				gridLimitatation.CellModels.Add("NullableInteger", nullableIntegerCell);

			gridLimitatation.QueryCellInfo += gridLimitatation_QueryCellInfo;
			gridLimitatation.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			gridLimitatation.RowCount = _presenter.ShiftCategories.Count;

			for (int i = 0; i < _presenter.ShiftCategories.Count; i++)
			{
				gridLimitatation[i + 1, 0].CellValue = _presenter.ShiftCategories[i].Description.Name;
				gridLimitatation[i + 1, 0].Tag = _presenter.ShiftCategories[i];
			}
			gridLimitatation.ActivateCurrentCellBehavior = GridCellActivateAction.DblClickOnCell;
			gridLimitatation.AutoSize = true;
			gridLimitatation.ReadOnly = !PrincipalAuthorization.Current_DONTUSE().IsPermitted(
					  DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
		}

		void gridLimitatation_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			IShiftCategory category = null;
			if (e.RowIndex > _presenter.ShiftCategories.Count)
				return;
			if (e.RowIndex > 0)
				category = _presenter.OnQueryShiftCategoryCellInfo(e.RowIndex);
			if (e.RowIndex == 0)
			{
				e.Style.CellValue = ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(e.RowIndex, e.ColIndex);
			}
			else if (e.RowIndex > 0 && e.ColIndex > 0 && e.ColIndex < 4)
			{
				e.Style.CellType = onQueryCellType(e.ColIndex);
				e.Style.CheckBoxOptions = new GridCheckBoxCellInfo("true", "false", "", false);
				e.Style.TriState = true;
				e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;

				if (category != null)
					e.Style.CellValue = _presenter.OnQueryLimitationCellInfo(category, e.ColIndex);
			}
			else if (e.RowIndex > 0 && e.ColIndex == 4)
			{
				e.Style.CellType = onQueryCellType(e.ColIndex);
				if (category != null)
				{
					int? value = _presenter.GetCurrentMaxNumberOf(category);
					if (!value.HasValue)
					{
						e.Style.CellValue = double.NaN;
						e.Style.Interior = new BrushInfo(PatternStyle.DarkUpwardDiagonal, Color.LightGray, Color.White);
					}
					else
					{
						e.Style.CellValue = value.Value;
					}
				}
			}
			else if (e.RowIndex > 0 && e.ColIndex == 0 && e.Style.Tag != null && category != null)
			{
				e.Style.CellValue = (category).Description.Name;
			}
		}

		public GridControl ShiftCategoryLimitationGrid
		{
			get { return gridLimitatation; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<IShiftCategory> ShiftCategories
		{
			get { return _shiftCategories; }
			set { _shiftCategories = value; }
		}

		public void SetSchedulePeriodList(IList<ISchedulePeriod> schedulePeriods)
		{
			gridLimitatation.Enabled = (schedulePeriods.Count > 0);
			_presenter.SetSchedulePeriodList(schedulePeriods);
			gridLimitatation.Invalidate(true);
			gridLimitatation.ReadOnly = !PrincipalAuthorization.Current_DONTUSE().IsPermitted(
					  DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
		}

		public void InitializePresenter()
		{
			_presenter.Initialize();
		}

		public void SetDirtySchedulePeriod(ISchedulePeriod schedulePeriod)
		{
			foreach (var adapter in _filteredPeopleHolder.SchedulePeriodGridViewCollection)
			{
				if (adapter.SchedulePeriod == schedulePeriod)
				{
					adapter.CanBold = true;
					_gridConstructor.View.Invalidate();
				}

				if (adapter.GridControl != null)
				{
					IList<SchedulePeriodChildModel> childAdapters = adapter.GridControl.Tag as
						 IList<SchedulePeriodChildModel>;

					if (childAdapters != null)
					{
						for (int i = 0; i < childAdapters.Count; i++)
						{
							if (childAdapters[i].SchedulePeriod == schedulePeriod)
							{
								childAdapters[i].CanBold = true;
								_gridConstructor.View.Invalidate();
							}
						}
					}
				}
			}
		}

		public void SetState(FilteredPeopleHolder filteredPeopleHolder, GridConstructor gridConstructor)
		{
			_filteredPeopleHolder = filteredPeopleHolder;
			_gridConstructor = gridConstructor;
			_gridViewBase = new EmptyGridView(gridLimitatation, filteredPeopleHolder, true);
		}

		public int ViewWidth()
		{
			return gridLimitatation.Cols.Size.GetTotal(0, 5);
		}

		private static string onQueryCellType(int colIndex)
		{
			string cellType;
			switch (colIndex)
			{
				case 0:
					cellType = "Header";
					break;
				case 1:
				case 2:
				case 3:
					cellType = "CheckBox";
					break;
				case 4:
					cellType = "NullableInteger";
					break;
				default:
					cellType = "Static";
					break;
			}
			return cellType;
		}

		private void gridLimitatation_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(
					  DefinedRaptorApplicationFunctionPaths.AllowPersonModifications)) return;
			if (e.RowIndex == -1)
				return;

			IShiftCategory category = _presenter.OnQueryShiftCategoryCellInfo(e.RowIndex);

			ShiftCategoryLimitationCombination combination = _presenter.CombinedLimitations[category];
			if (e.ColIndex == 1)
			{
				if (!combination.Limit.HasValue || combination.Limit.Value == false)
				{
					IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(category);
					if (combination.Weekly.HasValue)
						limitation.Weekly = combination.Weekly.Value;
					if (combination.MaxNumberOf.HasValue)
						limitation.MaxNumberOf = 0;
					_presenter.AddLimitation(limitation);
				}
				else
				{
					_presenter.RemoveLimitation(category);
				}
			}
			if (e.ColIndex == 2)
			{
				if (!combination.Weekly.HasValue || combination.Weekly.Value == false)
				{
					_presenter.SetWeekProperty(category, true);
				}
				else
				{
					_presenter.SetWeekProperty(category, false);
				}
			}
			if (e.ColIndex == 3)
			{
				if (!combination.Period.HasValue || combination.Period.Value == false)
				{
					_presenter.SetWeekProperty(category, false);
				}
				else
				{
					_presenter.SetWeekProperty(category, true);
				}
			}


			if (e.ColIndex == 4)
			{
				e.Style.CellType = "NullableInteger";
				if (string.IsNullOrEmpty(e.Style.CellValue.ToString()))
				{
					return;
				}

				double? nanTest = e.Style.CellValue as double?;
				if (nanTest.HasValue && nanTest.Value.Equals(double.NaN))
					return;

				_presenter.SetNumberOf(category, ((int?)e.Style.CellValue).Value);

			}
			gridLimitatation.Invalidate(true);
		}

		private void ReleaseManagedResources()
		{
			if (gridLimitatation != null)
			{
				gridLimitatation.ClipboardPaste -= gridLimitatation_ClipboardPaste;
			}
			
			gridLimitatation = null;
			_gridConstructor = null;
			_filteredPeopleHolder = null;
			_presenter.Dispose();
			_presenter = null;
			if (_gridViewBase != null)
			{
				_gridViewBase.Dispose();
				_gridViewBase = null;
			}

			_shiftCategories = null;
		}

		private void gridLimitatation_ClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
			_gridViewBase.ClipboardPaste(e);
		}
	}
}
