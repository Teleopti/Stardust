using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{

	public partial class ActivityControl : BaseUserControl, ISettingPage
	{
		private SFGridColumnGridHelper<IActivity> _gridColumnHelper;
		private readonly IDictionary<GridType, object> _sourceList;
	   // private IUnitOfWork _unitOfWork;
		private readonly string _newActivityName = string.Empty;
		private const string NewActivityNameFormat = "<{0} {1}>";
		private const string LessThanChar = "<";
		private const string GreaterThanChar = ">";
		private const string SpaceChar = " ";
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

		private void ButtonNewActivityClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Sets the row index to 0 when there's no data available in the source
			SetSelectedCellWhenNoSourceAvailable<IActivity>(GridType.Activity);

			// Adds tehe new source entity to repository
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var activityRepository = new ActivityRepository(uow);
				_gridColumnHelper.Add(activityRepository);
			}
			
			gridControlActivities.Invalidate();

			Cursor.Current = Cursors.Default;
		}

		private void ButtonAdvDeleteActivityClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Deletes the selected activities
			DeleteSelectedActivities();

			Cursor.Current = Cursors.Default;
		}

		private ReadOnlyCollection<SFGridColumnBase<IActivity>> ConfigureActivityGrid()
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
			gridColumns.Add(new SFGridCheckBoxColumn<IActivity>("AllowOverwrite",Resources.AllowMeetings ));

			gridColumns.Add(new SFGridDropDownEnumColumn<IActivity, ReportLevelDetailAdapter, ReportLevelDetail>("ReportLevelDetail", Resources.ReportLevel, ReportLevelDetails(), "DisplayName", "ReportLevelDetail"));
			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
			{
				var payrollColumn = new SFGridEditableTextColumn<IActivity>("PayrollCode", 20, Resources.PayrollCode) { AllowEmptyValue = true };
				gridColumns.Add(payrollColumn);
			}
			gridColumns.Add(new ActivityUpdatedReadOnlyTextColumn<IActivity>("UpdatedBy", Resources.UpdatedBy));
			gridColumns.Add(new ActivityUpdatedReadOnlyTextColumn<IActivity>("UpdatedTimeInUserPerspective", Resources.UpdatedOn));
			gridControlGroupingActivities.RowCount = GridRowCount(GridType.Activity);
			gridControlGroupingActivities.ColCount = gridColumns.Count - 1;  //col index starts on 0

			return new ReadOnlyCollection<SFGridColumnBase<IActivity>>(gridColumns);
		}

		private List<ReportLevelDetailAdapter> ReportLevelDetails()
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

		private IActivity CreateActivity()
		{
			int nextCount = GetNextActivityId();

			// Formats the name.
			var name = string.Format(CultureInfo.InvariantCulture, NewActivityNameFormat, _newActivityName, nextCount);

			IActivity newActivity = new Activity(name) {DisplayColor = Color.DodgerBlue, PayrollCode = string.Empty};
			_activitiesToAdd.Add(newActivity );
			return newActivity;
		}

		private void DeleteSelectedActivities()
		{
			// Gets the source data collection and grid type
			const GridType gridType = GridType.Activity;
			IList<IActivity> source = GetSource<IActivity>(gridType);
			IList<IActivity> itemsToDelete = _gridColumnHelper.FindSelectedItems();

			if (!IsReadyToDelete(source)) return;
			
			//--var activityRepository = new ActivityRepository(_unitOfWork);

			foreach (var activity in itemsToDelete)
			{
				// Removes the activity from the repository and the source data
				source.Remove(activity);
				_activitiesToBeDeleted.Add(activity );
			}

			_gridColumnHelper.SetSourceList(GetSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList());

			InvalidateGrid<IActivity>(gridType);
		}


		private int GetNextActivityId()
		{
			var activities = (IList<IActivity>)_sourceList[GridType.Activity];
			int parsedValue;
			var sortedArray = (from q in
									 ((from p in activities
									   where p.Description.Name.Contains(_newActivityName)
									   select p.Description.Name
									   .Replace(LessThanChar, string.Empty)
									   .Replace(GreaterThanChar, string.Empty)
									   .Replace(_newActivityName, string.Empty)
									   .Replace(SpaceChar, string.Empty)).ToList())
							   where string.IsNullOrEmpty(q) == false && Int32.TryParse(q, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsedValue)
								 select Int32.Parse(q, CultureInfo.CurrentCulture)).ToArray();

			return GetNextId(sortedArray);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.Configuration.ActivityControl.ShowMyErrorMessage(System.String,System.String)")]
		private bool IsReadyToDelete<T>(ICollection<T> source)
		{
			bool isReady = false;

			if (IsDataAvailable(source))
			{
				if (MessageDialogs.ShowQuestion(this, Resources.DeleteSelectedRowsQuestionmark, Resources.Message) == DialogResult.Yes)
				{
					isReady = true;
				}
			}

			return isReady;
		}

		// Part of fix for bug: 12922
		// Fix disabled in 309.
		//private bool AreUsedInAnySkill(IList<IActivity> itemsToDelete)
		//{
		//    SkillRepository skillRepository = new SkillRepository(_unitOfWork);
		//    ICollection<ISkill> foundSkills = skillRepository.FindAllWithActivities(itemsToDelete);
		//    if (foundSkills.Count > 0)
		//    {
		//        StringBuilder stringBuilder = new StringBuilder();
		//        stringBuilder.AppendLine(Resources.SelectedActivitiesCannotBeDeletedColon);
		//        stringBuilder.AppendLine();
		//        foreach (ISkill skill in foundSkills)
		//        {
		//            stringBuilder.AppendLine(skill.Name);
		//        }
		//        ViewBase.ShowInformationMessage(stringBuilder.ToString(), Resources.CannotDeleteSelectedActivities);
		//        return true;
		//    }
		//    return false;
		//}

		private static bool IsDataAvailable<T>(ICollection<T> source)
		{
			bool isDataExists = false;

			if ((source != null) && (source.Count > 0))
			{
				isDataExists = true;
			}

			return isDataExists;
		}

		private void InvalidateGrid<T>(GridType gridType)
		{
			// Gets the data source
			IList<T> source = GetSource<T>(gridType);

			if (source == null) return;
			GridControl grid = GetGridControl(gridType);

			grid.RowCount = source.Count;
			grid.Invalidate();
		}

		private GridControl GetGridControl(GridType gridType)
		{
			var grid = gridType == GridType.Activity ? gridControlActivities : gridControlGroupingActivities;

			return grid;
		}

		private static int GetNextId(int[] array)
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

		private void LoadSourceList(IUnitOfWork uow)
		{
			_sourceList.Clear();
			var groupingActivityRepository = new GroupingActivityRepository(uow);
			var activityRepository = new ActivityRepository(uow);
			_sourceList.Add(GridType.GroupingActivity, groupingActivityRepository.LoadAll());
			_sourceList.Add(GridType.Activity, activityRepository.LoadAllWithUpdatedBy());
             
            //gridControlActivities.Refresh();
		}

		private List<T> GetSource<T>(GridType gridType)
		{
			var source = (List<T>)_sourceList[gridType];
			return source;
		}

		private int GridRowCount(GridType gridType)
		{
			int sourceListCount = 0;
			int gridHeaderCount = 0;
			switch (gridType)
			{
				case GridType.Activity:
					gridHeaderCount = gridControlActivities.Rows.HeaderCount;
					var activityList = (IList<IActivity>)_sourceList[GridType.Activity];
					sourceListCount = activityList.Count;
					break;
				case GridType.GroupingActivity:
					gridHeaderCount = gridControlGroupingActivities.Rows.HeaderCount;
					var groupingActivityList = (IList<GroupingActivity>)_sourceList[GridType.GroupingActivity];
					sourceListCount = groupingActivityList.Count;
					break;
			}
			return (sourceListCount + gridHeaderCount);
		}

		private void SetSelectedCellWhenNoSourceAvailable<T>(GridType gridType)
		{
			// Gets the source
			IList<T> source = GetSource<T>(gridType);

			if (IsDataAvailable(source)) return;
			GridControl grid = GetGridControl(gridType);
			grid.CurrentCell.MoveTo(0, 0);
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();

			toolTip1.SetToolTip(buttonNewActivity, Resources.AddActivity);
			toolTip1.SetToolTip(buttonNewGroupingActivity, Resources.AddGroupingActivity);

			toolTip1.SetToolTip(buttonAdvDeleteActivity, Resources.Delete);
			toolTip1.SetToolTip(buttonAdvDeleteGroupingActivity, Resources.Delete);
		}

		public void InitializeDialogControl()
		{
			SetColors();
			SetTexts();
		}

		private void SetColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			gridControlGroupingActivities.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlGroupingActivities.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();

			gridControlActivities.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlActivities.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
		{
			GridHelper.GridStyle(gridControlGroupingActivities);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				LoadSourceList(uow);
			}
			ReadOnlyCollection<SFGridColumnBase<IActivity>> activityColumns = ConfigureActivityGrid();
			_gridColumnHelper = new SFGridColumnGridHelper<IActivity>(gridControlActivities,
								activityColumns,
								GetSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList()) {AllowExtendedCopyPaste = true};

			_gridColumnHelper.NewSourceEntityWanted += ColumnGridHelperNewSourceEntityWanted;

		}

		public void SaveChanges()
		{
			Persist();
		}
		public void Persist()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var activityRepository = new ActivityRepository(uow);
				
                foreach (var activity in (IList<IActivity>)_sourceList[GridType.Activity])
                {
                    var a = uow.Merge(activity);
                    LazyLoadingManager.Initialize(a.UpdatedBy);
                }
                foreach (var activity in (IList<GroupingActivity>)_sourceList[GridType.GroupingActivity])
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
				LoadSourceList(uow);
                _gridColumnHelper.SetSourceList(GetSource<IActivity>(GridType.Activity).OrderBy(a => a.Description.Name).ToList());
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

		

		void ColumnGridHelperNewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<IActivity> e)
		{
			e.SourceEntity = CreateActivity();
		}

		enum GridType
		{
			GroupingActivity,
			Activity,
		}

		private void ToolStripMenuItemAddFromClipboardClick(object sender, EventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_gridColumnHelper.AddFromClipboard(new ActivityRepository(uow));
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

		private void ActivityControlLayout(object sender, LayoutEventArgs e)
		{
			gridControlActivities.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			gridControlGroupingActivities.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}

		private void gridControlActivities_CellClick(object sender, GridCellClickEventArgs e)
		{
			var col = e.ColIndex;
		}
	}
}
