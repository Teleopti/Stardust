using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class RequestAllowanceView : BaseDialogForm, IRequestAllowanceView
	{
		private readonly RequestAllowancePresenter _presenter;
		private readonly ColumnEntityBinder<BudgetAbsenceAllowanceDetailModel> _entityBinder;
		private GridRowSection<BudgetAbsenceAllowanceDetailModel> _absenceSection;

		public RequestAllowanceView()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				labelBudgetGroup.Text = string.Format(CultureInfo.CurrentUICulture, "{0}{1}",
													  UserTexts.Resources.BudgetGroup, UserTexts.Resources.Colon);

				labelAllowance.Text = string.Format(CultureInfo.CurrentUICulture, "{0}{1}",
													  UserTexts.Resources.Allowance, UserTexts.Resources.Colon);
			}
			_entityBinder = new ColumnEntityBinder<BudgetAbsenceAllowanceDetailModel>(requestAllowanceGridControl);
			bindNavigationButton();
		}

		private void bindNavigationButton()
		{
			requestAllowanceGridControl.PreviousButtonClicked += requestAllowanceGridControlPreviousButtonClicked;
			requestAllowanceGridControl.NextButtonClicked += requestAllowanceGridControlNextButtonClicked;
		}

		public RequestAllowanceView(IBudgetGroup budgetGroup, DateOnly defaultDate) : this()
		{
			var currentUnitOfWork = new FromFactory(() => UnitOfWorkFactory.Current);
			_presenter = new RequestAllowancePresenter(this,
													   new RequestAllowanceModel(UnitOfWorkFactory.Current,
														   new BudgetDayRepository(currentUnitOfWork),
														   new BudgetGroupRepository(currentUnitOfWork),
														   new DefaultScenarioFromRepository(new ScenarioRepository(currentUnitOfWork)),
														   new ScheduleProjectionReadOnlyPersister(currentUnitOfWork)));

			_presenter.Initialize(budgetGroup, defaultDate);
			initializeGrid();
			_presenter.InitializeGridBinding();
			initialize();
		}

		private void initialize()
		{
			comboBoxAdvBudgetGroup.SelectedIndexChanged -= comboBoxAdvBudgetGroupSelectedIndexChanged;
			comboBoxAdvBudgetGroup.DataSource = _presenter.BudgetGroups();
			comboBoxAdvBudgetGroup.SelectedIndexChanged += comboBoxAdvBudgetGroupSelectedIndexChanged;
			comboBoxAdvBudgetGroup.SelectedItem = _presenter.SelectedBudgetGroup();
			comboBoxAdvBudgetGroup.DisplayMember = "Name";
			radioButtonShrinkedAllowance.Checked = true;
		}

		public IList<BudgetAbsenceAllowanceDetailModel> DataSource
		{
			set { _entityBinder.SetBinding(value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void initializeGrid()
		{
			var numCell = new NumericReadOnlyCellModel(requestAllowanceGridControl.Model) { NumberOfDecimals = 2 };
			var percentCell = new PercentReadOnlyCellModel(requestAllowanceGridControl.Model) { NumberOfDecimals = 2 };
			requestAllowanceGridControl.CellModels.Add("NumericReadOnlyCellModel", numCell);
			requestAllowanceGridControl.CellModels.Add("PercentReadOnlyCellModel", percentCell);

			_entityBinder.GridColors = new GridColors
			{
				ColorHolidayCell = ColorHelper.GridControlGridHolidayCellColor(),
				ColorHolidayHeader = ColorHelper.GridControlGridHolidayHeaderColor(),
				ColorReadOnlyCell = ColorHelper.DefaultTextColor
			};

			_entityBinder.AddRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
			{
				HeaderText = UserTexts.Resources.Allowance,
				ValueMember = new ModelProperty<BudgetAbsenceAllowanceDetailModel>("Allowance"),
				CellValueType = typeof(double),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_absenceSection = new GridRowSection<BudgetAbsenceAllowanceDetailModel>(_entityBinder);
			_entityBinder.AddRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
			{
				HeaderText = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", UserTexts.Resources.Used, UserTexts.Resources.Total),
				ValueMember = new ModelProperty<BudgetAbsenceAllowanceDetailModel>("UsedTotalAbsences"),
				CellValueType = typeof(double),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
			{
				HeaderText = UserTexts.Resources.AbsoluteDifference,
				ValueMember = new ModelProperty<BudgetAbsenceAllowanceDetailModel>("AbsoluteDifference"),
				CellValueType = typeof(double),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
			{
				HeaderText = UserTexts.Resources.RelativeDifference,
				ValueMember = new ModelProperty<BudgetAbsenceAllowanceDetailModel>("RelativeDifference"),
				CellValueType = typeof(double),
				CellModel = "PercentReadOnlyCellModel",
				ReadOnly = true
			});
			_entityBinder.AddRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
			{
				HeaderText = UserTexts.Resources.HeadCount,
				ValueMember = new ModelProperty<BudgetAbsenceAllowanceDetailModel>("TotalHeadCounts"),
				CellValueType = typeof(double),
				CellModel = "NumericReadOnlyCellModel",
				ReadOnly = true
			});
			requestAllowanceGridControl.Rows.HeaderCount = 1;
			requestAllowanceGridControl.Rows.FrozenCount = 1;
			requestAllowanceGridControl.DefaultColWidth = 60;
			_entityBinder.SetColumnParentHeaderMember(new ModelProperty<BudgetAbsenceAllowanceDetailModel>("Week"));
			_entityBinder.SetColumnHeaderMember(new ModelProperty<BudgetAbsenceAllowanceDetailModel>("Date"));
			requestAllowanceGridControl.QueryCellInfo += requestAllowanceGridControlQueryCellInfo;
		}

		private void requestAllowanceGridControlQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex != RequestAllowanceGridControl.NavigationButtonRowIndex) return;

			if (e.ColIndex == RequestAllowanceGridControl.PrevButtonColIndex)
			{
				e.Style.CellType = "PushButton";
				e.Style.CellAppearance = GridCellAppearance.Flat;
				e.Style.Description = "<";
			}
			else if (e.ColIndex == RequestAllowanceGridControl.NextButtonColIndex)
			{
				e.Style.CellType = "PushButton";
				e.Style.CellAppearance = GridCellAppearance.Flat;
				e.Style.Description = ">";
			}
		}

		private void radioButtonFullAllowanceCheckChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonFullAllowanceCheckChanged(radioButtonFullAllowance.Checked);
		}

		private void radioButtonShrinkedAllowanceCheckChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonShrinkedAllowanceCheckChanged(radioButtonShrinkedAllowance.Checked);
		}

		private void comboBoxAdvBudgetGroupSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnComboBoxAdvBudgetGroupSelectedIndexChanged(comboBoxAdvBudgetGroup.SelectedItem);
		}

		public void ReloadAbsenceSection()
		{
			_absenceSection.ClearRows();
			foreach (var absence in _presenter.Absences)
			{
				_absenceSection.InsertRow(new GridRow<BudgetAbsenceAllowanceDetailModel>
				{
					HeaderText = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.Used + " {0}", absence.Name),
					ValueMember = new ModelDictionaryProperty<BudgetAbsenceAllowanceDetailModel, double>("UsedAbsencesDictionary", absence.Id.GetValueOrDefault().ToString(), 0d),
					CellValueType = typeof(double),
					CellModel = "NumericReadOnlyCellModel",
					ReadOnly = true
				});
			}
			resizeWindow();
		}

		private void resizeWindow()
		{
			resizeRowHeaders();
			Width = requestAllowanceGridControl.ColWidths.GetTotal(0, requestAllowanceGridControl.ColCount) + 40;
		}

		private void resizeRowHeaders()
		{
			requestAllowanceGridControl.ColWidths.ResizeToFit(GridRangeInfo.Col(0), GridResizeToFitOptions.IncludeHeaders);
		}

		private void requestAllowanceGridControlNextButtonClicked(object sender, EventArgs e)
		{
			_presenter.OnRequestAllowanceGridControlNextButtonClicked();
		}

		private void requestAllowanceGridControlPreviousButtonClicked(object sender, EventArgs e)
		{
			_presenter.OnRequestAllowanceGridControlPreviousButtonClicked();
		}

		private void requestAllowanceViewLoad(object sender, EventArgs e)
		{
			Location = new Point(Owner.Location.X + (Owner.Width - Width) / 2, Owner.Location.Y + (Owner.Height - Height) / 2);
			ReloadAbsenceSection();
		}

		private void buttonRefreshClick(object sender, EventArgs e)
		{
			_presenter.OnRefreshButtonClicked();
		}
	}
}