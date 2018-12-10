using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
{
	public partial class BudgetGroupDayView : BaseUserControl, IBudgetGroupDayView, ISelectedBudgetDays
	{
		private readonly IEventAggregator _localEventAggregator;
		private readonly BudgetDayReassociator _budgetDayReassociator;
		private readonly IBudgetPermissionService _budgetPermissionService;
		private ColumnEntityBinder<IBudgetGroupDayDetailModel> _entityBinder;
		private GridRowSection<IBudgetGroupDayDetailModel> _shrinkageSection;
		private GridRowSection<IBudgetGroupDayDetailModel> _efficiencyShrinkageSection;
		private IList<double> _modifiedItems;
		private readonly IEventAggregator _globalEventAggregator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reassociator")]
		public BudgetGroupDayView(IEventAggregatorLocator eventAggregatorLocator, BudgetDayReassociator budgetDayReassociator, IBudgetPermissionService budgetPermissionService)
		{
			_localEventAggregator = eventAggregatorLocator.LocalAggregator();
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();
			_budgetDayReassociator = budgetDayReassociator;
			_budgetPermissionService = budgetPermissionService;
			InitializeComponent();
			InitializeGrid();
			SetTexts();
		}

		public BudgetGroupDayPresenter Presenter { get; set; }

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		public void Initialize()
		{
			EventSubscription();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_budgetDayReassociator.Reassociate();
				Presenter.Initialize();
			}

			SetupGridControl();

			budgetGroupDayViewMenu.Items[9].Enabled = false;
		}

		private void EventSubscription()
		{
			_localEventAggregator.GetEvent<ShrinkageRowAdded>().Subscribe(ShrinkageRowAdded);
			_localEventAggregator.GetEvent<EfficiencyShrinkageRowAdded>().Subscribe(EfficiencyShrinkageRowAdded);
			_localEventAggregator.GetEvent<ShrinkageRowsDeleted>().Subscribe(ShrinkageRowsDeleted);
			_localEventAggregator.GetEvent<EfficiencyShrinkageRowsDeleted>().Subscribe(EfficiencyShrinkageRowsDeleted);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Subscribe(GridUpdated);
			_localEventAggregator.GetEvent<LoadDataStarted>().Subscribe(LoadDataStarted);
			_localEventAggregator.GetEvent<LoadDataFinished>().Subscribe(LoadDataFinished);
			_localEventAggregator.GetEvent<BeginBudgetDaysUpdate>().Subscribe(OnBeginBudgetDaysUpdate);
			_localEventAggregator.GetEvent<EndBudgetDaysUpdate>().Subscribe(OnEndBudgetDaysUpdate);
			_localEventAggregator.GetEvent<ExitEditMode>().Subscribe(OnExitEditMode);
			_localEventAggregator.GetEvent<ViewsClipboardAction>().Subscribe(OnViewsClipboardAction);
			_localEventAggregator.GetEvent<UpdateShrinkageProperty>().Subscribe(UpdateShrinkageProperty);
			_localEventAggregator.GetEvent<UpdateEfficiencyShrinkageProperty>().Subscribe(UpdateEfficiencyShrinkageProperty);
			_globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Subscribe(budgetGroupNeedsRefresh);
		}

		private void budgetGroupNeedsRefresh(IBudgetGroup budgetGroup)
		{
			Presenter.UpdateBudgetGroup(budgetGroup);
			gridControlDayView.SuspendInvalidate();
			reloadShrinkageSections();
			gridControlDayView.ResumeInvalidate();
		}

		private void UpdateEfficiencyShrinkageProperty(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			Presenter.RecalculateAll();
			_efficiencyShrinkageSection.UpdateRowHeaderTextWithTag(customEfficiencyShrinkage, customEfficiencyShrinkage.ShrinkageName + " (%)");
			gridControlDayView.Invalidate();
		}

		private void UpdateShrinkageProperty(ICustomShrinkage customShrinkage)
		{
			Presenter.RecalculateAll();
			_shrinkageSection.UpdateRowHeaderTextWithTag(customShrinkage, customShrinkage.ShrinkageName + " (%)");
			gridControlDayView.Invalidate();
		}

		private void OnViewsClipboardAction(ClipboardActionOnViews clipboardActionOnViewsEventArgs)
		{
			if (clipboardActionOnViewsEventArgs.ViewType != this) return;
			switch (clipboardActionOnViewsEventArgs.ClipboardAction)
			{
				case ClipboardAction.Copy:
					gridControlDayView.CutPaste.Copy();
					break;

				case ClipboardAction.Cut:
					gridControlDayView.CutPaste.Cut();
					break;

				case ClipboardAction.Paste:
					{
						OnBeginBudgetDaysUpdate(true);
						gridControlDayView.CutPaste.Paste();
						OnEndBudgetDaysUpdate(true);
					}
					break;
			}
		}

		private void OnExitEditMode(bool obj)
		{
			if (gridControlDayView.CurrentCell != null)
				gridControlDayView.CurrentCell.EndEdit();
		}

		private void OnEndBudgetDaysUpdate(bool obj)
		{
			_entityBinder.EndDelayUpdates();
			Cursor.Current = DefaultCursor;
		}

		private void OnBeginBudgetDaysUpdate(bool obj)
		{
			Cursor.Current = Cursors.WaitCursor;
			_entityBinder.BeginDelayUpdates();
		}

		private void LoadDataFinished(string obj)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(LoadDataFinished), obj);
			}
			else
			{
				Cursor.Current = Cursors.Default;
				gridControlDayView.Enabled = true;
			}
		}

		private void LoadDataStarted(string obj)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(LoadDataStarted), obj);
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				gridControlDayView.Enabled = false;
			}
		}

		private void EfficiencyShrinkageRowsDeleted(IEnumerable<ICustomEfficiencyShrinkage> deletedRows)
		{
			deletedRows.ForEach(_efficiencyShrinkageSection.DeleteRowWithTag);
			Presenter.RecalculateAll();
			if (_efficiencyShrinkageSection.RowCount() == 0)
			{
				toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = false;
				toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = false;
			}
		}

		private void ShrinkageRowsDeleted(IEnumerable<ICustomShrinkage> deletedRows)
		{
			deletedRows.ForEach(_shrinkageSection.DeleteRowWithTag);
			Presenter.RecalculateAll();
			if (_shrinkageSection.RowCount() == 0)
			{
				toolStripMenuItemDeleteShrinkageRow.Enabled = false;
				toolStripMenuItemUpdateShrinkageRow.Enabled = false;
			}
		}

		private void EfficiencyShrinkageRowAdded(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			Presenter.AddEfficiencyShrinkageRow(customEfficiencyShrinkage);
		}

		private void ShrinkageRowAdded(ICustomShrinkage customShrinkage)
		{
			Presenter.AddShrinkageRow(customShrinkage);
		}

		//Fix later
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IBudgetGroupDayDetailModel> DataSource
		{
			get { throw new NotImplementedException(); } //return grid.DataSource
			set
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action<IList<IBudgetGroupDayDetailModel>>(_entityBinder.SetBinding), value);
				}
				else
				{
					_entityBinder.SetBinding(value);
				}
			}
		}

		private void InitializeGrid()
		{
			var numCell = new NumericReadOnlyCellModel(gridControlDayView.Model) { NumberOfDecimals = 2 };
			var percentCell = new PercentReadOnlyCellModel(gridControlDayView.Model) { NumberOfDecimals = 2 };
			var percentWithTwoDecimalsCell = new PercentCellModel(gridControlDayView.Model) { NumberOfDecimals = 2, MinMax = new MinMax<double>(0, 1) };
			var numericWithTwoDecimalsCell = new NumericCellModel(gridControlDayView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 99999999d };
			var nullableCellModelWithTwoDecimalsCell = new NullableNumericCellModel(gridControlDayView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 99999999d };
			var nullableNegativeCellModelWithTwoDecimalsCell = new NullableNumericCellModel(gridControlDayView.Model) { NumberOfDecimals = 2, MinValue = -99999999d, MaxValue = 99999999d };
			var restrictedValueForFte = new NumericCellModel(gridControlDayView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 12d };

			gridControlDayView.CellModels.Add("RestrictedValueForFTE", restrictedValueForFte);
			gridControlDayView.CellModels.Add("NumericReadOnlyCellModel", numCell);
			gridControlDayView.CellModels.Add("PercentReadOnlyCellModel", percentCell);
			gridControlDayView.CellModels.Add("PercentWithTwoDecimalsCell", percentWithTwoDecimalsCell);
			gridControlDayView.CellModels.Add("NumericWithTwoDecimalsCell", numericWithTwoDecimalsCell);
			gridControlDayView.CellModels.Add("NullableNumericWithTwoDecimalsCell", nullableCellModelWithTwoDecimalsCell);
			gridControlDayView.CellModels.Add("NullableNegativeNumericWithTwoDecimalsCell", nullableNegativeCellModelWithTwoDecimalsCell);
			gridControlDayView.TableStyle.CheckBoxOptions = new GridCheckBoxCellInfo("True", "False", "", false);

			_entityBinder = new ColumnEntityBinder<IBudgetGroupDayDetailModel>(gridControlDayView);
			_entityBinder.GridColors = new GridColors
			{
				ColorHolidayCell = ColorHelper.GridControlGridHolidayCellColor(),
				ColorHolidayHeader = ColorHelper.GridControlGridHolidayHeaderColor(),
				ReadOnlyBackgroundBrush = ColorHelper.ReadOnlyBackgroundBrush,
				ColorReadOnlyCell = Color.Black
			};
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.FulltimeEquivalentHoursPerDay,
				ValueMember =
					new ModelProperty<IBudgetGroupDayDetailModel>("FulltimeEquivalentHours"),
				CellValueType = typeof(double),
				CellModel = "RestrictedValueForFTE"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.StaffEmployed,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("StaffEmployed"),
				CellValueType = typeof(double?),
				CellModel = "NullableNumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.AttritionRate + " (%)"),
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("AttritionRate"),
				CellValueType = typeof(Percent),
				CellModel = "PercentWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.Recruitment,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("Recruitment"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.GrossStaff,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("GrossStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.Contractors + " ({0})",
							  UserTexts.Resources.TimeTypeHours),
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("Contractors"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_shrinkageSection = new GridRowSection<IBudgetGroupDayDetailModel>(_entityBinder);
			_shrinkageSection.GridRowSectionSelectionChanged += shrinkageSection_GridRowSectionSelectionChanged;
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.DaysOffPerWeek,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("DaysOffPerWeek"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.Closed,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("IsClosed"),
				CellValueType = typeof(bool),
				CellModel = "CheckBox",
				HorizontalAlignment = GridHorizontalAlignment.Center
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.NetStaff,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("NetStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.NetStaffFCAdj,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("NetStaffFcAdj"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText =
					string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.Overtime + " ({0})",
							  UserTexts.Resources.TimeTypeHours),
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("OvertimeHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.StudentHours,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("StudentsHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_efficiencyShrinkageSection = new GridRowSection<IBudgetGroupDayDetailModel>(_entityBinder, _shrinkageSection);
			_efficiencyShrinkageSection.GridRowSectionSelectionChanged += efficiencyShrinkageSection_GridRowSectionSelectionChanged;
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.BudgetedStaff,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("BudgetedStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.ForecastedHours,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("ForecastedHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell",
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.ForecastedStaff,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("ForecastedStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.Difference,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("Difference"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
			{
				HeaderText = UserTexts.Resources.DifferencePercent,
				ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("DifferencePercent"),
				CellModel = "PercentReadOnlyCellModel",
				ReadOnly = true
			});

			if (_budgetPermissionService.IsAllowancePermitted)
			{
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.BudgetedLeave,
					ValueMember =
												 new ModelProperty<IBudgetGroupDayDetailModel>("BudgetedLeave"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.BudgetedSurplus,
					ValueMember =
												 new ModelProperty<IBudgetGroupDayDetailModel>("BudgetedSurplus"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.AbsenceExtra,
					ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("AbsenceExtra"),
					CellValueType = typeof(double),
					CellModel = "NullableNegativeNumericWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.AbsenceOverride,
					ValueMember =
												 new ModelProperty<IBudgetGroupDayDetailModel>("AbsenceOverride"),
					CellValueType = typeof(double),
					CellModel = "NullableNegativeNumericWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.FullAllowance,
					ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("FullAllowance"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = string.Format(UserTexts.Resources.AbsenceThreshold + " (%)"),
					ValueMember =
												 new ModelProperty<IBudgetGroupDayDetailModel>("AbsenceThreshold"),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = UserTexts.Resources.ShrinkedAllowance,
					ValueMember = new ModelProperty<IBudgetGroupDayDetailModel>("ShrinkedAllowance"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
			}

			gridControlDayView.Rows.HeaderCount = 1;

			_entityBinder.SetColumnHeaderMember(new ModelProperty<IBudgetGroupDayDetailModel>("Date"));
			_entityBinder.SetColumnParentHeaderMember(new ModelProperty<IBudgetGroupDayDetailModel>("Week"));

			gridControlDayView.DefaultColWidth = 60;
		}

		private void efficiencyShrinkageSection_GridRowSectionSelectionChanged(object sender, GridRowSectionSelectionChangedEventArgs e)
		{
			toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = e.SectionSelected;
			toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = e.SectionSelected;
		}

		private void shrinkageSection_GridRowSectionSelectionChanged(object sender, GridRowSectionSelectionChangedEventArgs e)
		{
			toolStripMenuItemDeleteShrinkageRow.Enabled = e.SectionSelected;
			toolStripMenuItemUpdateShrinkageRow.Enabled = e.SectionSelected;
		}

		public void AddShrinkageRow(ICustomShrinkage customShrinkage)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<ICustomShrinkage>(AddShrinkageRow), customShrinkage);
			}
			else
			{
				if (customShrinkage == null) throw new ArgumentNullException("customShrinkage");
				_shrinkageSection.InsertRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = string.Format(CultureInfo.CurrentCulture, customShrinkage.ShrinkageName + " (%)"),
					ValueMember =
														new ModelDynamicProperty<IBudgetGroupDayDetailModel, Percent, ICustomShrinkage>(
														OnGetShrinkageModelValue, OnSetShrinkageModelValue, customShrinkage),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell",
					Tag = customShrinkage
				});
			}
		}

		private void OnSetShrinkageModelValue(IBudgetGroupDayDetailModel dayDetail, ICustomShrinkage customShrinkageRow, object value)
		{
			dayDetail.SetShrinkage(customShrinkageRow, (Percent)value);
		}

		private object OnGetShrinkageModelValue(IBudgetGroupDayDetailModel dayDetail, ICustomShrinkage customShrinkageRow)
		{
			return dayDetail.GetShrinkage(customShrinkageRow);
		}

		public void AddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<ICustomEfficiencyShrinkage>(AddEfficiencyShrinkageRow), customEfficiencyShrinkage);
			}
			else
			{
				if (customEfficiencyShrinkage == null) throw new ArgumentNullException("customEfficiencyShrinkage");
				_efficiencyShrinkageSection.InsertRow(new GridRow<IBudgetGroupDayDetailModel>
				{
					HeaderText = customEfficiencyShrinkage.ShrinkageName + " (%)",
					ValueMember =
																new ModelDynamicProperty
																<IBudgetGroupDayDetailModel, Percent, ICustomEfficiencyShrinkage>
																(OnGetEfficiencyShrinkageModelValue, OnSetEfficiencyShrinkageModelValue,
																 customEfficiencyShrinkage),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell",
					Tag = customEfficiencyShrinkage
				});
			}
		}

		private void OnSetEfficiencyShrinkageModelValue(IBudgetGroupDayDetailModel dayDetail, ICustomEfficiencyShrinkage customEfficiencyShrinkageRow, object value)
		{
			dayDetail.SetEfficiencyShrinkage(customEfficiencyShrinkageRow, (Percent)value);
		}

		private object OnGetEfficiencyShrinkageModelValue(IBudgetGroupDayDetailModel dayDetail, ICustomEfficiencyShrinkage customEfficiencyShrinkageRow)
		{
			return dayDetail.GetEfficiencyShrinkage(customEfficiencyShrinkageRow);
		}

		private void toolStripMenuItemAddShrinkageRow_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<AddShrinkageRow>().Publish(string.Empty);
		}

		private void toolStripMenuItemAddEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<AddEfficiencyShrinkageRow>().Publish(string.Empty);
		}

		private void gridControlDayView_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			_shrinkageSection.IsInsideSection(e.Range);
			_efficiencyShrinkageSection.IsInsideSection(e.Range);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(true);
			budgetGroupDayViewMenu.Items[9].Enabled = true;
		}

		private void toolStripMenuItemDeleteShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _shrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);
			var itemsToDelete = new List<ICustomShrinkage>();
			foreach (var gridRow in rows)
			{
				itemsToDelete.Add((ICustomShrinkage)gridRow.Tag);
			}
			_localEventAggregator.GetEvent<DeleteCustomShrinkages>().Publish(itemsToDelete);
		}

		private void toolStripMenuItemDeleteEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _efficiencyShrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);
			var itemsToDelete = new List<ICustomEfficiencyShrinkage>();
			foreach (var gridRow in rows)
			{
				itemsToDelete.Add((ICustomEfficiencyShrinkage)gridRow.Tag);
			}
			_localEventAggregator.GetEvent<DeleteCustomEfficiencyShrinkages>().Publish(itemsToDelete);
		}

		public IEnumerable<IBudgetGroupDayDetailModel> Find()
		{
			return _entityBinder.CurrentSelection().SelectedEntities();
		}

		private void toolStripMenuItemLoadForcastToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadForecastedHours>().Publish("from day");
		}

		private void toolStripMenuItemLoadStaffEmployed_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadStaffEmployed>().Publish("from day");
		}

		private void toolStripMenuItemModifySelection_Paint(object sender, PaintEventArgs e)
		{
			bool enableMenuItem;
			_modifiedItems = new List<double>();
			_modifiedItems.Clear();
			GridHelper.ModifySelectionEnabled(gridControlDayView, out _modifiedItems, out enableMenuItem);
			toolStripMenuItemModifySelection.Enabled = enableMenuItem;
		}

		private void toolStripMenuItemModifySelection_Click(object sender, EventArgs e)
		{
			var modifySelectedList = _modifiedItems;
			var numbers = new ModifyCalculator(modifySelectedList);
			using (var modifySelection = new ModifySelectionView(numbers))
			{
				if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
				var receivedValues = modifySelection.ModifiedList;
				GridHelper.ModifySelectionInput(gridControlDayView, receivedValues);
			}
		}

		private void SetupGridControl()
		{
			gridControlDayView.ColWidths.ResizeToFit(GridRangeInfo.Col(0));
			_entityBinder.RangeChanged.RangeChanging += RangeChanged_RangeChanging;
			_entityBinder.RangeChanged.RangeChanged += RangeChanged_RangeChanged;
		}

		private void RangeChanged_RangeChanged(object sender, EventArgs e)
		{
			OnEndBudgetDaysUpdate(true);
		}

		private void RangeChanged_RangeChanging(object sender, EventArgs e)
		{
			OnBeginBudgetDaysUpdate(true);
		}

		private void gridControlDayView_Layout(object sender, LayoutEventArgs e)
		{
			gridControlDayView.Invalidate();
			budgetGroupDayViewMenu.Items[9].Enabled = false;
		}

		private void gridControlDayView_ClipboardCopy(object sender, GridCutPasteEventArgs e)
		{
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Publish(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
		}

		private void gridControlDayView_ClipboardCut(object sender, GridCutPasteEventArgs e)
		{
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Publish(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
		}

		private void toolStriUpdateShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _shrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);

			var shrinkage = rows.First();
			if (shrinkage != null)
			{
				_localEventAggregator.GetEvent<ChangeCustomShrinkage>().Publish((ICustomShrinkage)shrinkage.Tag);
			}
		}

		private void toolStripMenuItemUpdateEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _efficiencyShrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);

			var efficiency = rows.First();
			if (efficiency != null)
			{
				_localEventAggregator.GetEvent<ChangeCustomEfficiencyShrinkage>().Publish((ICustomEfficiencyShrinkage)efficiency.Tag);
			}
		}

		private void reloadShrinkageSections()
		{
			_shrinkageSection.ClearRows();
			_efficiencyShrinkageSection.ClearRows();
			Presenter.InitializeShrinkages();
			Presenter.InitializeEfficiencyShrinkages();
		}

		private void budgetGroupDayViewMenu_Opening(object sender, CancelEventArgs e)
		{
			if (_efficiencyShrinkageSection.RowCount() > 0)
			{
				var efficiencyRows = _efficiencyShrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);
				if (efficiencyRows.Any())
				{
					toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = true;
					toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = true;
				}
				else
				{
					toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = false;
					toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = false;
				}
			}

			if (_shrinkageSection.RowCount() > 0)
			{
				var shrinkageRows = _shrinkageSection.GetSelectedRows(gridControlDayView.Selections.Ranges);

				if (shrinkageRows.Any())
				{
					toolStripMenuItemUpdateShrinkageRow.Enabled = true;
					toolStripMenuItemDeleteShrinkageRow.Enabled = true;
				}
				else
				{
					toolStripMenuItemUpdateShrinkageRow.Enabled = false;
					toolStripMenuItemDeleteShrinkageRow.Enabled = false;
				}
			}
		}

		private void GridUpdated(bool b)
		{
			if (!b)
				budgetGroupDayViewMenu.Items[9].Enabled = false;
		}
	}
}