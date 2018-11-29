using System;
using System.Collections.Generic;
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
	public partial class BudgetGroupWeekView : BaseUserControl, IBudgetGroupWeekView, ISelectedBudgetDays
	{
		private readonly BudgetDayReassociator _budgetDayReassociator;
		private readonly IBudgetPermissionService _budgetPermissionService;
		private readonly IEventAggregator _localEventAggregator;
		private ColumnEntityBinder<BudgetGroupWeekDetailModel> _entityBinder;
		private GridRowSection<BudgetGroupWeekDetailModel> _shrinkageSection;
		private GridRowSection<BudgetGroupWeekDetailModel> _efficiencyShrinkageSection;
		private IList<double> _modifiedItems;
		private readonly IEventAggregator _globalEventAggregator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reassociator")]
		public BudgetGroupWeekView(IEventAggregatorLocator eventAggregatorLocator, BudgetDayReassociator budgetDayReassociator, IBudgetPermissionService budgetPermissionService)
		{
			_budgetDayReassociator = budgetDayReassociator;
			_budgetPermissionService = budgetPermissionService;
			_localEventAggregator = eventAggregatorLocator.LocalAggregator();
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();
			InitializeComponent();
			InitializeGrid();

			SetTexts();
		}

		public BudgetGroupWeekPresenter Presenter { get; set; }

		public void Initialize()
		{
			EventSubscription();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_budgetDayReassociator.Reassociate();
				Presenter.Initialize();
			}

			SetGridSizes();
			budgetGroupWeekViewMenu.Items[9].Enabled = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<BudgetGroupWeekDetailModel> DataSource
		{
			get { throw new NotImplementedException(); }
			set
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action<IList<BudgetGroupWeekDetailModel>>(_entityBinder.SetBinding), value);
				}
				else
				{
					_entityBinder.SetBinding(value);
				}
			}
		}

		private void SetGridSizes()
		{
			gridControlWeekView.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			_entityBinder.RangeChanged.RangeChanged += OnRangeChanged;
			_entityBinder.RangeChanged.RangeChanging += OnRangeChanging;
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
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
			_localEventAggregator.GetEvent<ExitEditMode>().Subscribe(OnExitEditMode);
			_localEventAggregator.GetEvent<BeginBudgetDaysUpdate>().Subscribe(OnBeginBudgetDaysUpdate);
			_localEventAggregator.GetEvent<EndBudgetDaysUpdate>().Subscribe(OnEndBudgetDaysUpdate);
			_localEventAggregator.GetEvent<ViewsClipboardAction>().Subscribe(OnViewsClipboardAction);
			_localEventAggregator.GetEvent<UpdateShrinkageProperty>().Subscribe(UpdateShrinkageProperty);
			_localEventAggregator.GetEvent<UpdateEfficiencyShrinkageProperty>().Subscribe(UpdateEfficiencyShrinkageProperty);
			_globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Subscribe(budgetGroupNeedsRefresh);
		}

		private void budgetGroupNeedsRefresh(IBudgetGroup budgetGroup)
		{
			Presenter.UpdateBudgetGroup(budgetGroup);
			reloadShrinkageSections();
		}

		private void reloadShrinkageSections()
		{
			_shrinkageSection.ClearRows();
			_efficiencyShrinkageSection.ClearRows();
			Presenter.InitializeShrinkages();
			Presenter.InitializeEfficiencyShrinkages();
		}

		private void UpdateEfficiencyShrinkageProperty(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			_efficiencyShrinkageSection.UpdateRowHeaderTextWithTag(customEfficiencyShrinkage, customEfficiencyShrinkage.ShrinkageName + " (%)");
			gridControlWeekView.Invalidate();
		}

		private void UpdateShrinkageProperty(ICustomShrinkage customShrinkage)
		{
			_shrinkageSection.UpdateRowHeaderTextWithTag(customShrinkage, customShrinkage.ShrinkageName + " (%)");
			gridControlWeekView.Invalidate();
		}

		private void OnViewsClipboardAction(ClipboardActionOnViews clipboardActionOnViewsEventArgs)
		{
			if (clipboardActionOnViewsEventArgs.ViewType != this) return;
			switch (clipboardActionOnViewsEventArgs.ClipboardAction)
			{
				case ClipboardAction.Copy:
					gridControlWeekView.CutPaste.Copy();
					break;

				case ClipboardAction.Cut:
					gridControlWeekView.CutPaste.Cut();
					break;

				case ClipboardAction.Paste:
					{
						OnBeginBudgetDaysUpdate(true);
						gridControlWeekView.CutPaste.Paste();
						OnEndBudgetDaysUpdate(true);
					}
					break;
			}
		}

		private void OnEndBudgetDaysUpdate(bool obj)
		{
			_entityBinder.EndDelayUpdates();
		}

		private void OnBeginBudgetDaysUpdate(bool obj)
		{
			_entityBinder.BeginDelayUpdates();
		}

		private void OnExitEditMode(bool obj)
		{
			if (gridControlWeekView.CurrentCell != null)
				gridControlWeekView.CurrentCell.EndEdit();
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
				gridControlWeekView.Enabled = true;
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
				gridControlWeekView.Enabled = false;
			}
		}

		private void EfficiencyShrinkageRowsDeleted(IEnumerable<ICustomEfficiencyShrinkage> deletedRows)
		{
			deletedRows.ForEach(_efficiencyShrinkageSection.DeleteRowWithTag);
			if (_efficiencyShrinkageSection.RowCount() == 0)
			{
				toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = false;
				toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = false;
			}
		}

		private void ShrinkageRowsDeleted(IEnumerable<ICustomShrinkage> deletedRows)
		{
			deletedRows.ForEach(_shrinkageSection.DeleteRowWithTag);
			if (_shrinkageSection.RowCount() == 0)
			{
				toolStripMenuItemDeleteShrinkageRow.Enabled = false;
				toolStripMenuItemUpdateShrinkageRow.Enabled = false;
			}
		}

		private void EfficiencyShrinkageRowAdded(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			Presenter.UpdateEfficiencyShrinkageRow(customEfficiencyShrinkage);
		}

		private void ShrinkageRowAdded(ICustomShrinkage customShrinkage)
		{
			Presenter.UpdateShrinkageRow(customShrinkage);
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
				_shrinkageSection.InsertRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = customShrinkage.ShrinkageName + " (%)",
					ValueMember =
						new ModelDynamicProperty<BudgetGroupWeekDetailModel, Percent, ICustomShrinkage>(
						OnGetShrinkageModelValue, OnSetShrinkageModelValue, customShrinkage),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell",
					Tag = customShrinkage
				});
			}
		}

		private void OnSetShrinkageModelValue(BudgetGroupWeekDetailModel weekDetailModel, ICustomShrinkage customShrinkage, object value)
		{
			weekDetailModel.SetCustomShrinkage(customShrinkage, (Percent)value);
		}

		private object OnGetShrinkageModelValue(BudgetGroupWeekDetailModel weekDetailModel, ICustomShrinkage customShrinkage)
		{
			return weekDetailModel.GetCustomShrinkage(customShrinkage);
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
				_efficiencyShrinkageSection.InsertRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = customEfficiencyShrinkage.ShrinkageName + " (%)",
					ValueMember =
						new ModelDynamicProperty
						<BudgetGroupWeekDetailModel, Percent, ICustomEfficiencyShrinkage>
						(OnGetEfficiencyShrinkageModelValue, OnSetEfficiencyShrinkageModelValue,
						 customEfficiencyShrinkage),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell",
					Tag = customEfficiencyShrinkage
				});
			}
		}

		private void OnSetEfficiencyShrinkageModelValue(BudgetGroupWeekDetailModel weekDetailModel, ICustomEfficiencyShrinkage customEfficiencyShrinkageRow, object value)
		{
			weekDetailModel.SetCustomEfficiencyShrinkage(customEfficiencyShrinkageRow, (Percent)value);
		}

		private object OnGetEfficiencyShrinkageModelValue(BudgetGroupWeekDetailModel weekDetailModel, ICustomEfficiencyShrinkage customEfficiencyShrinkageRow)
		{
			return weekDetailModel.GetEfficiencyShrinkage(customEfficiencyShrinkageRow);
		}

		private void InitializeGrid()
		{
			var numCell = new NumericReadOnlyCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2 };
			var percentCell = new PercentReadOnlyCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2 };
			var percentWithTwoDecimalsCell = new PercentCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2, MinMax = new MinMax<double>(0, 1) };
			var restrictedValueForFte = new NumericCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 12d };
			var numericWithTwoDecimalsCell = new NumericCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 99999999d };
			var nullableCellModelWithTwoCecimal = new NullableNumericCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2, MinValue = 0, MaxValue = 99999999d };
			var nullableNegativeCellModelWithTwoDecimalsCell = new NullableNumericCellModel(gridControlWeekView.Model) { NumberOfDecimals = 2, MinValue = -99999999d, MaxValue = 99999999d };

			gridControlWeekView.CellModels.Add("NumericReadOnlyCellModel", numCell);
			gridControlWeekView.CellModels.Add("PercentReadOnlyCellModel", percentCell);
			gridControlWeekView.CellModels.Add("RestrictedValueForFTE", restrictedValueForFte);
			gridControlWeekView.CellModels.Add("PercentWithTwoDecimalsCell", percentWithTwoDecimalsCell);
			gridControlWeekView.CellModels.Add("NumericWithTwoDecimalsCell", numericWithTwoDecimalsCell);
			gridControlWeekView.CellModels.Add("NullableNumericWithTwoDecimalsCell", nullableCellModelWithTwoCecimal);
			gridControlWeekView.CellModels.Add("NullableNegativeNumericWithTwoDecimalsCell", nullableNegativeCellModelWithTwoDecimalsCell);

			_entityBinder = new ColumnEntityBinder<BudgetGroupWeekDetailModel>(gridControlWeekView);
			_entityBinder.GridColors = new GridColors
			{
				ColorHolidayCell = ColorHelper.GridControlGridHolidayCellColor(),
				ColorHolidayHeader = ColorHelper.GridControlGridHolidayHeaderColor(),
				ReadOnlyBackgroundBrush = ColorHelper.ReadOnlyBackgroundBrush,
				ColorReadOnlyCell = Color.Black
			};
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.FulltimeEquivalentHoursPerDay,
				ValueMember =
					new ModelProperty<BudgetGroupWeekDetailModel>("FulltimeEquivalentHours"),
				CellValueType = typeof(double),
				CellModel = "RestrictedValueForFTE"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.StaffEmployed,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("StaffEmployed"),
				CellValueType = typeof(double?),
				CellModel = "NullableNumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.AttritionRate + " (%)"),
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("AttritionRate"),
				CellValueType = typeof(Percent),
				CellModel = "PercentWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.Recruitment,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("Recruitment"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.GrossStaff,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("GrossStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.Contractors + " ({0})",
							  UserTexts.Resources.TimeTypeHours),
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("Contractors"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});

			_shrinkageSection = new GridRowSection<BudgetGroupWeekDetailModel>(_entityBinder);
			_shrinkageSection.GridRowSectionSelectionChanged += shrinkageSection_GridRowSectionSelectionChanged;

			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.DaysOffPerWeek,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("DaysOffPerWeek"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.NetStaff,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("NetStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});

			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.Overtime + " ({0})",
							  UserTexts.Resources.TimeTypeHours),
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("OvertimeHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.StudentHours,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("StudentsHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell"
			});
			_efficiencyShrinkageSection = new GridRowSection<BudgetGroupWeekDetailModel>(_entityBinder, _shrinkageSection);
			_efficiencyShrinkageSection.GridRowSectionSelectionChanged += efficiencyShrinkageSection_GridRowSectionSelectionChanged;

			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.NetNetStaff,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("NetNetStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});

			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.ForecastedHours,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("ForecastedHours"),
				CellValueType = typeof(double),
				CellModel = "NumericWithTwoDecimalsCell",
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.ForecastedStaff,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("ForecastedStaff"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.Difference,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("Difference"),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
			{
				HeaderText = UserTexts.Resources.DifferencePercent,
				ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("DifferencePercent"),
				CellModel = "PercentReadOnlyCellModel",
				ReadOnly = true
			});

			if (_budgetPermissionService.IsAllowancePermitted)
			{
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.BudgetedLeave,
					ValueMember =
												 new ModelProperty<BudgetGroupWeekDetailModel>("BudgetedLeave"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.BudgetedSurplus,
					ValueMember =
												 new ModelProperty<BudgetGroupWeekDetailModel>("BudgetedSurplus"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.AbsenceExtra,
					ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("AbsenceExtra"),
					CellValueType = typeof(double),
					CellModel = "NullableNegativeNumericWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.AbsenceOverride,
					ValueMember =
												 new ModelProperty<BudgetGroupWeekDetailModel>("AbsenceOverride"),
					CellValueType = typeof(double),
					CellModel = "NullableNegativeNumericWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.FullAllowance,
					ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("FullAllowance"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = string.Format(UserTexts.Resources.AbsenceThreshold + " (%)"),
					ValueMember =
												 new ModelProperty<BudgetGroupWeekDetailModel>("AbsenceThreshold"),
					CellValueType = typeof(Percent),
					CellModel = "PercentWithTwoDecimalsCell"
				});
				_entityBinder.AddRow(new GridRow<BudgetGroupWeekDetailModel>
				{
					HeaderText = UserTexts.Resources.ShrinkedAllowance,
					ValueMember = new ModelProperty<BudgetGroupWeekDetailModel>("ShrinkedAllowance"),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
			}

			gridControlWeekView.Rows.HeaderCount = 1;
			_entityBinder.SetColumnParentHeaderMember(new ModelProperty<BudgetGroupWeekDetailModel>("MonthYear"));
			_entityBinder.SetColumnHeaderMember(new ModelProperty<BudgetGroupWeekDetailModel>("Week"));
		}

		private void OnRangeChanging(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<BeginBudgetDaysUpdate>().Publish(true);
		}

		private void OnRangeChanged(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<EndBudgetDaysUpdate>().Publish(true);
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

		private void toolStripMenuItemAddShrinkageRow_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<AddShrinkageRow>().Publish(string.Empty);
		}

		private void toolStripMenuItemAddEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<AddEfficiencyShrinkageRow>().Publish(string.Empty);
		}

		private void toolStripMenuItemDeleteShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _shrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);
			var itemsToDelete = new List<ICustomShrinkage>();
			foreach (var gridRow in rows)
			{
				itemsToDelete.Add((ICustomShrinkage)gridRow.Tag);
			}
			_localEventAggregator.GetEvent<DeleteCustomShrinkages>().Publish(itemsToDelete);
		}

		private void toolStripMenuItemDeleteEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _efficiencyShrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);
			var itemsToDelete = new List<ICustomEfficiencyShrinkage>();
			foreach (var gridRow in rows)
			{
				itemsToDelete.Add((ICustomEfficiencyShrinkage)gridRow.Tag);
			}
			_localEventAggregator.GetEvent<DeleteCustomEfficiencyShrinkages>().Publish(itemsToDelete);
		}

		private void toolStripMenuItemLoadForecast_Click(object sender, EventArgs e)
		{
			if (gridControlWeekView.CurrentCell != null)
				gridControlWeekView.CurrentCell.CancelEdit();
			_localEventAggregator.GetEvent<LoadForecastedHours>().Publish("from week");
		}

		private void toolStripMenuItemLoadStaffEmployed_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadStaffEmployed>().Publish("from week");
		}

		private void toolStripMenuItemModifySelection_Paint(object sender, PaintEventArgs e)
		{
			bool enableMenuItem;
			_modifiedItems = new List<double>();
			_modifiedItems.Clear();
			GridHelper.ModifySelectionEnabled(gridControlWeekView, out _modifiedItems, out enableMenuItem);
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
				GridHelper.ModifySelectionInput(gridControlWeekView, receivedValues);
			}
		}

		private void gridControlWeekView_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			_shrinkageSection.IsInsideSection(e.Range);
			_efficiencyShrinkageSection.IsInsideSection(e.Range);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(true);
			budgetGroupWeekViewMenu.Items[9].Enabled = true;
		}

		public IEnumerable<IBudgetGroupDayDetailModel> Find()
		{
			var selectedWeeks = _entityBinder.CurrentSelection().SelectedEntities();
			var selectedBudgetDays = from w in selectedWeeks
									 from b in w.BudgetDays
									 select b;
			return selectedBudgetDays;
		}

		private void gridControlWeekView_ClipboardCopy(object sender, GridCutPasteEventArgs e)
		{
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Publish(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
		}

		private void gridControlWeekView_ClipboardCut(object sender, GridCutPasteEventArgs e)
		{
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Publish(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
		}

		private void budgetGroupWeekViewMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (_efficiencyShrinkageSection.RowCount() > 0)
			{
				var efficiencyRows = _efficiencyShrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);
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
				var shrinkageRows = _shrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);

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

		private void toolStripRenameShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _shrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);

			var shrinkage = rows.First();
			if (shrinkage != null)
			{
				_localEventAggregator.GetEvent<ChangeCustomShrinkage>().Publish((ICustomShrinkage)shrinkage.Tag);
			}
		}

		private void toolStripMenuItemRenameEfficiencyShrinkageRow_Click(object sender, EventArgs e)
		{
			var rows = _efficiencyShrinkageSection.GetSelectedRows(gridControlWeekView.Selections.Ranges);

			var efficiency = rows.First();
			if (efficiency != null)
			{
				_localEventAggregator.GetEvent<ChangeCustomEfficiencyShrinkage>().Publish((ICustomEfficiencyShrinkage)efficiency.Tag);
			}
		}

		private void GridUpdated(bool b)
		{
			if (!b)
				budgetGroupWeekViewMenu.Items[9].Enabled = false;
		}
	}
}