using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{

	public partial class ActivityControl : BaseUserControl, ISettingPage
	{
		private SFGridColumnGridHelper<IActivity> _gridColumnHelper;
		private readonly IDictionary<GridType, object> _sourceList;
		private readonly string _newActivityName = string.Empty;
		private const string newActivityNameFormat = "<{0} {1}>";
		private const string lessThanChar = "<";
		private const string greaterThanChar = ">";
		private const string spaceChar = " ";
		private readonly List<ReportLevelDetailAdapter> _reportLevelDetailAdapters;
		private readonly IList<IActivity> _activitiesToBeDeleted;
		private readonly IList<IActivity> _activitiesToAdd;

		public ActivityControl()
		{
			InitializeComponent();
			_newActivityName = Resources.NewActivity;
			_reportLevelDetailAdapters = new List<ReportLevelDetailAdapter>();
			_sourceList = new Dictionary<GridType, object>();
			_activitiesToBeDeleted = new List<IActivity>();
			_activitiesToAdd = new List<IActivity>();
		}

		private void buttonNewActivityClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Sets the row index to 0 when there's no data available in the source
			setSelectedCellWhenNoSourceAvailable<IActivity>(GridType.Activity);

			_gridColumnHelper.Add();
			
			gridControlActivities.Invalidate();

			Cursor.Current = Cursors.Default;
		}

		private void buttonAdvDeleteActivityClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Deletes the selected activities
			deleteSelectedActivities();

			Cursor.Current = Cursors.Default;
		}

		private ReadOnlyCollection<SFGridColumnBase<IActivity>> configureActivityGrid()
		{
			IList<SFGridColumnBase<IActivity>> gridColumns = new List<SFGridColumnBase<IActivity>>();

			gridControlActivities.CellModels.Add("DescriptionNameCell", new DescriptionNameCellModel(gridControlActivities.Model));
			gridControlActivities.CellModels.Add("ColorPickerCell", new ColorPickerCellModel(gridControlActivities.Model));
			gridControlActivities.Rows.HeaderCount = 0;

			gridColumns.Add(new SFGridRowHeaderColumn<IActivity>(string.Empty));
			gridColumns.Add(new SFGridDescriptionNameColumn<IActivity>("Description", Resources.Name));
			gridColumns.Add(new SFGridColorPickerColumn<IActivity>("DisplayColor", Resources.Color));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("InContractTime", Resources.IsContractTime));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("InWorkTime", Resources.IsWorkTime));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("InPaidTime", Resources.IsPaidTime));
			
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("RequiresSkill", Resources.RequiresSkill));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("RequiresSeat", Resources.RequiresSeat));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("InReadyTime", Resources.InReadyTime));
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("AllowOverwrite",Resources.Overwritable));

			gridColumns.Add(new SFGridDropDownEnumColumn<IActivity, ReportLevelDetailAdapter, ReportLevelDetail>("ReportLevelDetail", Resources.ReportLevel, reportLevelDetails(), "DisplayName", "ReportLevelDetail"));
			if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
			{
				var payrollColumn = new SFGridEditableTextColumn<IActivity>("PayrollCode", 20, Resources.PayrollCode) { AllowEmptyValue = true };
				gridColumns.Add(payrollColumn);
			}
			gridColumns.Add(new ActivityUpdatedReadOnlyTextColumn<IActivity>("UpdatedBy", Resources.UpdatedBy));
			gridColumns.Add(new ActivityUpdatedReadOnlyTextColumn<IActivity>("UpdatedTimeInUserPerspective", Resources.UpdatedOn));
			
			

			return new ReadOnlyCollection<SFGridColumnBase<IActivity>>(gridColumns);

			
		}

		private List<ReportLevelDetailAdapter> reportLevelDetails()
		{
			if (_reportLevelDetailAdapters.Count == 0)
			{
				IList<KeyValuePair<ReportLevelDetail, string>> multiplicatorTypeCollection = LanguageResourceHelper.TranslateEnumToList<ReportLevelDetail>();
				foreach (var multiplicatorTypeView in
					multiplicatorTypeCollection.Select(keyValuePair => new ReportLevelDetailAdapter(keyValuePair.Value, keyValuePair.Key)))
				{
					_reportLevelDetailAdapters.Add(multiplicatorTypeView);
				}
			}
			return _reportLevelDetailAdapters;
		}

		private IActivity createActivity()
		{
			int nextCount = getNextActivityId();

			// Formats the name.
			var name = string.Format(CultureInfo.InvariantCulture, newActivityNameFormat, _newActivityName, nextCount);

			IActivity newActivity = new Activity(name) {DisplayColor = Color.DodgerBlue, PayrollCode = string.Empty};
			_activitiesToAdd.Add(newActivity );
			return newActivity;
		}

		private void deleteSelectedActivities()
		{
			// Gets the source data collection and grid type
			const GridType gridType = GridType.Activity;
			IList<IActivity> source = getSource<IActivity>(gridType);
			IList<IActivity> itemsToDelete = _gridColumnHelper.FindSelectedItems();

			if (!isReadyToDelete(source)) return;
			
			//--var activityRepository = new ActivityRepository(_unitOfWork);

			foreach (var activity in itemsToDelete)
			{
				// Removes the activity from the repository and the source data
				source.Remove(activity);
				_activitiesToBeDeleted.Add(activity );
			}

			_gridColumnHelper.SetSourceList(getSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList());

			invalidateGrid<IActivity>(gridType);
		}


		private int getNextActivityId()
		{
			var activities = (IList<IActivity>)_sourceList[GridType.Activity];
			var sortedArray = (from q in
									 ((from p in activities
									   where p.Description.Name.Contains(_newActivityName)
									   select p.Description.Name
									   .Replace(lessThanChar, string.Empty)
									   .Replace(greaterThanChar, string.Empty)
									   .Replace(_newActivityName, string.Empty)
									   .Replace(spaceChar, string.Empty)).ToList())
							   where string.IsNullOrEmpty(q) == false && Int32.TryParse(q, NumberStyles.Integer, CultureInfo.CurrentCulture, out _)
								 select Int32.Parse(q, CultureInfo.CurrentCulture)).ToArray();

			return getNextId(sortedArray);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.Configuration.ActivityControl.ShowMyErrorMessage(System.String,System.String)")]
		private bool isReadyToDelete<T>(ICollection<T> source)
		{
			bool isReady = false;

			if (isDataAvailable(source))
			{
				if (MessageDialogs.ShowQuestion(this, Resources.DeleteSelectedRowsQuestionmark, Resources.Message) == DialogResult.Yes)
				{
					isReady = true;
				}
			}

			return isReady;
		}

		private static bool isDataAvailable<T>(ICollection<T> source)
		{
			bool isDataExists = (source != null) && (source.Count > 0);

			return isDataExists;
		}

		private void invalidateGrid<T>(GridType gridType)
		{
			// Gets the data source
			IList<T> source = getSource<T>(gridType);

			if (source == null) return;
			GridControl grid = getGridControl();

			grid.RowCount = source.Count;
			grid.Invalidate();
		}

		private GridControl getGridControl()
		{
			var grid =  gridControlActivities;

			return grid;
		}

		private static int getNextId(int[] array)
		{

			int nextId = 1;

			if (!array.IsEmpty())
			{
				Array.Sort(array);

				// Adds 1 to last number.
				nextId = array[(array.Length - 1)] + 1;
			}

			return nextId;
		}

		private void loadSourceList(IUnitOfWork uow)
		{
			_sourceList.Clear();
			var activityRepository = ActivityRepository.DONT_USE_CTOR(uow);
			_sourceList.Add(GridType.Activity, activityRepository.LoadAllWithUpdatedBy().Where(a => a.IsOutboundActivity == false && !(a is IMasterActivity)).ToList());
		}

		private List<T> getSource<T>(GridType gridType)
		{
			var source = (List<T>)_sourceList[gridType];
			return source;
		}

		private void setSelectedCellWhenNoSourceAvailable<T>(GridType gridType)
		{
			IList<T> source = getSource<T>(gridType);

			if (isDataAvailable(source)) return;
			GridControl grid = getGridControl();
			grid.CurrentCell.MoveTo(0, 0);
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonNewActivity, Resources.AddActivity);
			toolTip1.SetToolTip(buttonAdvDeleteActivity, Resources.Delete);
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
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

			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			gridControlActivities.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlActivities.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				loadSourceList(uow);
			}
			ReadOnlyCollection<SFGridColumnBase<IActivity>> activityColumns = configureActivityGrid();
			_gridColumnHelper = new SFGridColumnGridHelper<IActivity>(gridControlActivities,
								activityColumns,
								getSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList(),false) {AllowExtendedCopyPaste = true};

			_gridColumnHelper.NewSourceEntityWanted += columnGridHelperNewSourceEntityWanted;
			
		}

		public void SaveChanges()
		{
			Persist();
		}
		public void Persist()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var activityRepository = ActivityRepository.DONT_USE_CTOR(uow);

				foreach (var activity in (IList<IActivity>)_sourceList[GridType.Activity])
				{
					var a = uow.Merge(activity);
					LazyLoadingManager.Initialize(a.UpdatedBy);
				}			
				foreach (var activity in _activitiesToBeDeleted)
				{
					activityRepository.Remove(activity);
				}
				foreach (var activity in _activitiesToAdd)
				{
					activityRepository.Add(activity);
				}
				uow.PersistAll();
				_activitiesToBeDeleted.Clear();
				_activitiesToAdd.Clear();
				loadSourceList(uow);
				_gridColumnHelper.SetSourceList(getSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList());
			}
			
		}

		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.Activity;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		void columnGridHelperNewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<IActivity> e)
		{
			e.SourceEntity = createActivity();
		}

		enum GridType
		{
			Activity,
		}

		private void toolStripMenuItemAddFromClipboardClick(object sender, EventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_gridColumnHelper.AddFromClipboard(ActivityRepository.DONT_USE_CTOR(uow));
			}
			gridControlActivities.Invalidate();
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.Activity; }
		}

		private void activityControlLayout(object sender, LayoutEventArgs e)
		{
			gridControlActivities.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}
		
	}
}
