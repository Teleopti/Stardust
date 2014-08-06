using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class FairnessValuesControl : BaseUserControl, ISettingPage
	{
		private SFGridColumnGridHelper<JusticeValuesView> _gridColumnHelper;
		readonly IList<JusticeValuesView> _source = new List<JusticeValuesView>();
		private IUnitOfWork _unitOfWork;

		public FairnessValuesControl()
		{
			InitializeComponent();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
		}

		public void Persist()
		{}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
		{
			loadSourceList();

			var columns = configureGrid();
			_gridColumnHelper = new SFGridColumnGridHelper<JusticeValuesView>(grid, columns, _source as List<JusticeValuesView>, false)
									{AllowExtendedCopyPaste = false};
		}

		public void SaveChanges()
		{}

		public void Unload()
		{
			_gridColumnHelper = null;
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.JusticeValues;
		}

		public void OnShow()
		{
		}

		private void loadSourceList()
		{
			var repository = new ShiftCategoryRepository(_unitOfWork);
			var source = repository.LoadAll();

			_source.Clear();
			foreach (var shiftCategory in source.OrderBy(c => c.Description.Name).ToList())
			{
				if (shiftCategory.DayOfWeekJusticeValues.Count != 7)
					shiftCategory.ReinitializeDayOfWeekDictionary();
				_source.Add(new JusticeValuesView(shiftCategory));
			}
		}

		private ReadOnlyCollection<SFGridColumnBase<JusticeValuesView>> configureGrid()
		{
			IList<SFGridColumnBase<JusticeValuesView>> gridColumns = new List<SFGridColumnBase<JusticeValuesView>>();

			var cell = new NumericCellModel(grid.Model) {MinValue = 0, MaxValue = 999};

			grid.CellModels.Add("IntegerCellModel", cell);

			grid.Rows.HeaderCount = 0;
			grid.Cols.HeaderCount = 1;
			// Grid must have a Header column
			gridColumns.Add(new SFGridRowHeaderColumn<JusticeValuesView>(string.Empty));

			gridColumns.Add(new SFGridReadOnlyTextColumn<JusticeValuesView>("Name", Resources.ShiftCategoryHeader));
			var days = DateHelper.GetDaysOfWeek(
					TeleoptiPrincipal.Current.Regional.Culture);
			foreach (var dayOfWeek in days)
			{
				gridColumns.Add(new SFGridIntegerCellColumn<JusticeValuesView>(dayOfWeek.ToString(), getDayNameShort(dayOfWeek)));
			}

			grid.RowCount = _source.Count + grid.Rows.HeaderCount;
			grid.ColCount = gridColumns.Count - 1; //col index starts on 0
			grid.Cols.Hidden[0] = true;
			return new ReadOnlyCollection<SFGridColumnBase<JusticeValuesView>>(gridColumns);
		}

		private static string getDayNameShort(DayOfWeek day)
		{
			return TeleoptiPrincipal.Current.Regional.UICulture.
				DateTimeFormat.GetShortestDayName(day);
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			grid.BackColor = ColorHelper.GridControlGridInteriorColor();
			grid.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();

		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.JusticeValues; }
		}
	}
}
