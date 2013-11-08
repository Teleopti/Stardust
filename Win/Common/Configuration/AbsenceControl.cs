
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
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class AbsenceControl : BaseUserControl, ISettingPage
    {
        private SFGridColumnGridHelper<AbsenceView> _gridColumnHelper;
        private readonly IDictionary<GridType, object> _sourceList;
        private readonly TypedBindingCollection<ITracker> _trackerCollection = new TypedBindingCollection<ITracker>();
        private readonly TypedBindingCollection<TrackerView> _trackerAdopterCollection =
            new TypedBindingCollection<TrackerView>();
        private readonly string _newEntityeName = Resources.NewAbsence;
        private readonly IList<AbsenceView> _absenceViewsToBeDeleted;

        private const string NewAbsenceNameFormat = "<{0} {1}>";
        private const string LessThanChar = "<";
        private const string GreaterThanChar = ">";
        private const string SpaceChar = " ";
        private const int DefaultColumnIndex = 1;
        private const int RowIndexMappingVelue = 1;
        private const int EmptySourceCount = 0;
        private const int ColumnListCountMappingValue = 1;
        private const int EmptyHeaderCount = 0;
        private const int ItemIdMappingIndex = 1;
        private const int DefaultCellColumnIndex = 0;
        private const int DefaultCellRowIndex = 0;

        public AbsenceControl()
        {
            InitializeComponent();

            _sourceList = new Dictionary<GridType, object>();
            _absenceViewsToBeDeleted = new List<AbsenceView>();

            // Loads the tracker collection
            loadTrackerCollection();
        }

        private void ButtonAddAdvAbsenceClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Sets the row index to 0 when there's no data available in the source
            setSelectedCellWhenNoSourceAvailable<AbsenceView>(GridType.Absence);
            _gridColumnHelper.Add(null);
            gridControlAbsences.Invalidate();

            Cursor.Current = Cursors.Default;
        }

        private void ButtonAdvDeleteAbsenceClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            deleteSelectedAbsence();
            Cursor.Current = Cursors.Default;
        }

        private void ToolStripMenuItem3Click(object sender, EventArgs e)
        {
            var clipHandler = GridHelper.ConvertClipboardToClipHandler();
            using (var s = new GridRangeStyle(gridControlAbsences.RowCount + RowIndexMappingVelue, DefaultColumnIndex))
            {
                var dif = clipHandler.RowSpan();

                for (var i = 0; i < dif; i++)
                {
                    buttonAdvAddAbsence.PerformClick();
                }

                var range = s.Range;
                //loop all rows in selection, step with height in clip
                for (var index = range.Top; index <= range.Bottom; index = index + clipHandler.RowSpan())
                {
                    handlePaste(clipHandler, range, index, index);
                }
            }
        }



        #region Grouping Absence

        /// <remarks>
        /// Currently we are not providing the absence grouping. 
        /// Therefore these code has been commented.
        /// </remarks>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Cursor.Current = Cursors.WaitCursor;

            //// Sets the row index to 0 when there's no data available in the source
            //SetSelectedCellWhenNoSourceAvailable<GroupingAbsence>(GridType.GroupingAbsence);

            //GroupingAbsenceRepository groupingAbsenceRepository = new GroupingAbsenceRepository(unitOfWork);
            //_gridCoulmnHelperBase.GetSFGridColumnGridHelper<GroupingAbsence>(gridGroupingAbsences).Add(groupingAbsenceRepository);

            //gridGroupingAbsences.Invalidate();

            //Cursor.Current = Cursors.Default;
        }

        /// <remarks>
        /// Currently we are not providing the absence grouping. 
        /// Therefore these code has been commented.
        /// </remarks>
        private void buttonAdv1_Click(object sender, EventArgs e)
        {
            //Cursor.Current = Cursors.WaitCursor;
            //IList<int> _selectedList = GetSelectedRowsToBeDeleted(GridType.GroupingAbsence);

            //if (ShowMyErrorMessage(UserTexts.Resources.DeleteSelectedRowsQuestionmark, "Message") == DialogResult.Yes)
            //{
            //    GroupingAbsenceRepository ga = new GroupingAbsenceRepository(unitOfWork);
            //    if (_selectedList != null && _selectedList.Count > 0)
            //    {
            //        IList<GroupingAbsence> _source = GetSource<GroupingAbsence>(GridType.GroupingAbsence);
            //        IList<GroupingAbsence> _toBeDeleted = new List<GroupingAbsence>();
            //        for (int i = 0; i <= (_selectedList.Count - 1); i++)
            //        {
            //            _toBeDeleted.Add(_source[_selectedList[i]]);
            //        }

            //        foreach (GroupingAbsence _groupingAbsence in _toBeDeleted)
            //        {
            //            _source.Remove(_groupingAbsence);
            //            ga.Remove(_groupingAbsence);
            //        }

            //        gridGroupingAbsences.RowCount = _source.Count;
            //    }
            //}

            //gridGroupingAbsences.Invalidate();
            //Cursor.Current = Cursors.Default;
        }

        #endregion

        private int GetNextAbsenceId()
        {
            var absenceList = (IList<AbsenceView>)_sourceList[GridType.Absence];
            int parsedValue; 
            var sortedArray = (from q in
                                   ((from p in absenceList
                                     where p.Description.Name.Contains(_newEntityeName)
                                     select p.Description.Name
                                         .Replace(LessThanChar, string.Empty)
                                         .Replace(GreaterThanChar, string.Empty)
                                         .Replace(_newEntityeName, string.Empty)
                                         .Replace(SpaceChar, string.Empty)).ToList())

                               where string.IsNullOrEmpty(q) == false && Int32.TryParse(q, NumberStyles.Integer,CultureInfo.CurrentCulture, out parsedValue)
                               select Int32.Parse(q, CultureInfo.CurrentCulture)).ToArray();

            return getNextId(sortedArray);
        }

        private AbsenceView createAbsence()
        {
            // Formats the name.
            var nextCount = GetNextAbsenceId();
            var name = getNewEntityName(_newEntityeName, nextCount);

            var newAbsence = new Absence
                                     {
                                         Description = new Description(name),
                                         DisplayColor = Color.DodgerBlue,
                                         Tracker = defaultTracker.Tracker,
                                         PayrollCode = string.Empty
                                     };

            return new AbsenceView(newAbsence, false);
        }

        private void deleteSelectedAbsence()
        {
            // Gets the source data collection and grid type
            const GridType gridType = GridType.Absence;
            IList<AbsenceView> source = getSource<AbsenceView>(gridType);

        	if (!isReadyToDelete(source)) return;
        	IList<AbsenceView> itemsToDelete =
        		_gridColumnHelper.FindSelectedItems();

        	foreach (var absenceView in itemsToDelete)
        	{
        		if (absenceView.Id.HasValue)
        			_absenceViewsToBeDeleted.Add(absenceView); // Only add items to deleted list if they exist in db.

        		source.Remove(absenceView);
        	}

        	invalidateGrid<AbsenceView>(gridType);
        }



        #region Grouping Absence

        // Need to uncomment this when we are provigng absence grouping

        ///// <summary>
        ///// Gets the next group absence ID.
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: 
        ///// Created date: 
        ///// </remarks>
        //private int GetNextGroupAbsenceID()
        //{
        //    IList<GroupingAbsence> groupAbsenceList = (IList<GroupingAbsence>)_sourceList[GridType.GroupingAbsence];
        //    int[] sortedArray = (from q in
        //                             ((from p in groupAbsenceList
        //                               where p.Description.Name.Contains(NewEntityeName)
        //                               select p.Description.Name
        //                               .Replace(LessThanChar, string.Empty)
        //                               .Replace(GreaterThanChar, string.Empty)
        //                               .Replace(NewEntityeName, string.Empty)
        //                               .Replace(SpaceChar, string.Empty)).ToList())
        //                         where string.IsNullOrEmpty(q) == false
        //                         select Int32.Parse(q, CultureInfo.CurrentUICulture)).ToArray();

        //    return GetNextID(sortedArray);
        //}

        ///// <summary>
        ///// Creates the group absence.
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: 
        ///// Created date: 
        ///// </remarks>
        //private GroupingAbsence CreateGroupAbsence()
        //{
        //    // Formats the name.
        //    int nextCount = GetNextGroupAbsenceID();
        //    string name = GetNewEntityName(NewEntityeName, nextCount);
        //    GroupingAbsence newGroupAnsence = new GroupingAbsence(name);

        //    return newGroupAnsence;
        //}

        #endregion

        private void invalidateGrid<T>(GridType gridType)
        {
            // Gets the data source
            IList<T> source = getSource<T>(gridType);

        	if (source == null) return;
        	var grid = getGridControl(gridType);

        	grid.RowCount = source.Count;
        	grid.Invalidate();
        }

        private GridControl getGridControl(GridType gridType)
        {
        	var grid = gridType == GridType.Absence ? gridControlAbsences : gridControlGroupingAbsences;

        	return grid;
        }

		private bool isReadyToDelete<T>(ICollection<T> source)
        {
            var isReady = false;

            if (isDataAvailable(source))
            {
				if (MessageDialogs.ShowQuestion(this, Resources.DeleteSelectedRowsQuestionmark,
        	                                   Resources.Message) == DialogResult.Yes)
                {
                    isReady = true;
                }
            }

            return isReady;
        }

        private static bool isDataAvailable<T>(ICollection<T> source)
        {
            return (source != null) && (source.Count > EmptySourceCount);
        }

        private ReadOnlyCollection<SFGridColumnBase<T>> configureGrid<T>(GridType type)
        {
            // Holds he column list for the grid control
            IList<SFGridColumnBase<T>> gridColumns = new List<SFGridColumnBase<T>>();
            // Gets the relevant grid control
            var gridControl = getGridControl(type);

            // Adds the cell models to the grid control
            addCellmodels(gridControl);
            // Set the header count for the grid control
            gridControl.Rows.HeaderCount = EmptyHeaderCount;
            // Adds the header column for the grid control
            gridColumns.Add(new SFGridRowHeaderColumn<T>(string.Empty));

            switch (type)
            {
                case GridType.GroupingAbsence:
                    createColumnsForGroupingAbsenceGrid(gridColumns);
                    break;

                case GridType.Absence:
                    createColumnsForAbsenceGrid(gridColumns);
                    break;
            }

            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("CreatedBy", Resources.CreatedBy));
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("CreatedTimeInUserPerspective", Resources.CreatedOn));
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("UpdatedBy", Resources.UpdatedBy));
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("UpdatedTimeInUserPerspective", Resources.UpdatedOn));
            

            gridControl.RowCount = gridRowCount(type);
            gridControl.ColCount = (gridColumns.Count - ColumnListCountMappingValue);

            return new ReadOnlyCollection<SFGridColumnBase<T>>(gridColumns);
        }

        private static void addCellmodels(GridControl gridControl)
        {
            // Adds the cell models to the grid control
            gridControl.CellModels.Add("NumericCell", new NumericCellModel(gridControl.Model));
            gridControl.CellModels.Add("DescriptionNameCell", new DescriptionNameCellModel(gridControl.Model));
            gridControl.CellModels.Add("DescriptionShortNameCellModel",
                                       new DescriptionShortNameCellModel(gridControl.Model));

            gridControl.CellModels.Add("ColorPickerCell", new ColorPickerCellModel(gridControl.Model));

            gridControl.CellModels.Add("StaticDropDownCell", new DropDownCellStaticModel(gridControl.Model));
            gridControl.CellModels.Add("IgnoreCell", new IgnoreCellModel(gridControl.Model));

        }

        private void createColumnsForAbsenceGrid<T>(ICollection<SFGridColumnBase<T>> gridColumns)
        {
            gridColumns.Add(new SFGridDescriptionNameColumn<T>("Description", Resources.Name));
            var shortName = new SFGridDescriptionShortNameColumn<T>("Description", Resources.ShortName, 150, false, 2)
                            	{AllowEmptyValue = true};
        	gridColumns.Add(shortName);
            gridColumns.Add(new SFGridColorPickerColumn<T>("DisplayColor", Resources.Color, null));

            gridColumns.Add(new SFGridCheckBoxColumn<T>("InContractTime", Resources.IsContractTime));
            gridColumns.Add(new SFGridCheckBoxColumn<T>("InWorkTime", Resources.IsWorkTime));
            gridColumns.Add(new SFGridCheckBoxColumn<T>("InPaidTime", Resources.IsPaidTime));
            if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
            {
                var payrollColumn = new SFGridEditableTextColumn<T>("PayrollCode", 20, Resources.PayrollCode) { AllowEmptyValue = true };
                gridColumns.Add(payrollColumn);
            }
            gridColumns.Add(new SFGridByteCellColumn<T>("Priority", Resources.Priority, null));
            gridColumns.Add(new SFGridCheckBoxColumn<T>("Requestable", UserTexts.Resources.UseForRequests));
            var trackerColumn = new SFGridDropDownColumn<T, TrackerView>("Tracker", Resources.TrackerTypeHeader, _trackerAdopterCollection,
                                                      "Description", null, typeof(TrackerView))
                                	{UseDisablePropertyCheck = true};
        	gridColumns.Add(trackerColumn);
            gridColumns.Add(new SFGridCheckBoxColumn<T>("Confidential", Resources.Confidential));
        }

        private static void createColumnsForGroupingAbsenceGrid<T>(ICollection<SFGridColumnBase<T>> gridColumns)
        {
            gridColumns.Add(new SFGridDescriptionNameColumn<T>("Description", Resources.Name));
            gridColumns.Add(new SFGridDescriptionNameColumn<T>("Description", Resources.ShortName));
        }

        private int gridRowCount(GridType gridType)
        {
            // Gets the relevant grid control
            var gridControl = getGridControl(gridType);

            var sourceListCount = EmptySourceCount;
            var gridHeaderCount = gridControl.Rows.HeaderCount;

            switch (gridType)
            {
                case GridType.GroupingAbsence:
                    var activityList = (IList<GroupingAbsence>)_sourceList[gridType];
                    sourceListCount = activityList.Count;
                    break;

                case GridType.Absence:
                    var groupingActivityList = (IList<AbsenceView>)_sourceList[gridType];
                    sourceListCount = groupingActivityList.Count;
                    break;
            }

            return (sourceListCount + gridHeaderCount);
        }

        private void loadSourceList()
        {
            using (var myUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var groupingAbsenceRepository = new GroupingAbsenceRepository(myUow);
                _sourceList.Add(GridType.GroupingAbsence, groupingAbsenceRepository.LoadAll());
            }
            _sourceList.Add(GridType.Absence, getAbsenceViews());
        }

        private static IList<AbsenceView> getAbsenceViews()
        {
            // Holds the absence view collection.
            IList<AbsenceView> absenceViewCollection = new List<AbsenceView>();

            using (var myUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                // Loads the absences.
                var absenceRepository = new AbsenceRepository(myUow);
                var absenceCollection = absenceRepository.LoadAll();

                // Get those absences that are used by person account trackers.
                var absencesUsedByPersonAccount = getAbsencesUsedByPersonAccount(myUow);

                foreach (Absence absence in absenceCollection)
                {
                    // lazy load
                    if (!LazyLoadingManager.IsInitialized(absence.CreatedBy))
                        LazyLoadingManager.Initialize(absence.CreatedBy);
                    if (!LazyLoadingManager.IsInitialized(absence.UpdatedBy))
                        LazyLoadingManager.Initialize(absence.UpdatedBy);

                    // Instantiates the absence view relevant for the 
                    // givenabsence and add it to the collection.
                    absenceViewCollection.Add(new AbsenceView(absence, absencesUsedByPersonAccount.Contains(absence)));
                }
            }

            return absenceViewCollection.OrderBy(a => a.Description.Name).ToList();
        }


        private static IList<IAbsence> getAbsencesUsedByPersonAccount(IUnitOfWork uow)
        {
            var repository = new AbsenceRepository(uow);
            return repository.FindAbsenceTrackerUsedByPersonAccount();
        }

        private List<T> getSource<T>(GridType gridType)
        {
            var source = (List<T>)_sourceList[gridType];
            return source;
        }

        private static int getNextId(int[] array)
        {
            var nextId = ItemIdMappingIndex;

            if (!array.IsEmpty())
            {
                Array.Sort(array);

                // Adds 1 to last number.
                nextId = array[(array.Length - ItemIdMappingIndex)] + ItemIdMappingIndex;
            }

            return nextId;
        }

        private void loadTrackerCollection()
        {
            _trackerCollection.Clear();

            // Adds the default tracker to the adapter collection
            defaultTracker = new TrackerView(TrackerView.DefaultTracker);
            _trackerAdopterCollection.Add(defaultTracker);

            var trackers = Tracker.AllTrackers();

            foreach (var tracker in trackers)
            {
                _trackerAdopterCollection.Add(new TrackerView(tracker));
                _trackerCollection.Add(tracker);
            }
        }
        private TrackerView defaultTracker { get; set; }

        private static string getNewEntityName(string name, int itemIndex)
        {
            return string.Format(CultureInfo.InvariantCulture, NewAbsenceNameFormat, name, itemIndex);
        }

        private void setSelectedCellWhenNoSourceAvailable<T>(GridType gridType)
        {
            // Gets the source
            IList<T> source = getSource<T>(gridType);

        	if (isDataAvailable(source)) return;
        	var grid = getGridControl(gridType);
        	grid.CurrentCell.MoveTo(DefaultCellRowIndex, DefaultCellColumnIndex);
        }

        private void handlePaste(ClipHandler clipHandler, GridRangeInfo range, int cellRangeIndex, int row)
        {
            for (var columnIndex = 1; columnIndex <= gridControlAbsences.ColCount;
                columnIndex = columnIndex + clipHandler.ColSpan())
            {
                var col = columnIndex;

            	if (row <= gridControlAbsences.Rows.HeaderCount || col <= gridControlAbsences.Cols.HeaderCount) continue;
            	foreach (var clip in clipHandler.ClipList)
            	{
            		//check clip fits inside selected range, rows
            		if (GridHelper.IsPasteRangeOk(range, gridControlAbsences, clip, cellRangeIndex, columnIndex))
            		{
            			Paste(clip, row + clip.RowOffset, col + clip.ColOffset);
            		}
            	}
            }
        }

        public virtual void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }

            var gridStyleInfo = gridControlAbsences[rowIndex, columnIndex];
            var clipValue = (string)clip.ClipObject;

            if (clipValue.Length <= gridStyleInfo.MaxLength || gridStyleInfo.MaxLength == 0)
            {
                gridStyleInfo.ApplyFormattedText(clipValue);
            }
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonAdv1, Resources.Delete);
            toolTip1.SetToolTip(buttonAdvDeleteAbsence, Resources.Delete);
            toolTip1.SetToolTip(buttonAdvAddAbsence, Resources.AddAbsence);
            toolTip1.SetToolTip(btnAdd, Resources.AddGroupingAbsence);
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

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            if (labelSubHeader2 != null) labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            gridControlGroupingAbsences.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlGroupingAbsences.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();

            gridControlAbsences.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlAbsences.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
			  
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
        {
            GridHelper.GridStyle(gridControlGroupingAbsences);
            loadSourceList();

            var absenceColumns = configureGrid<AbsenceView>(GridType.Absence);

            _gridColumnHelper = new SFGridColumnGridHelper<AbsenceView>(gridControlAbsences,
                               absenceColumns,
                               getSource<AbsenceView>(GridType.Absence)) {AllowExtendedCopyPaste = true};

        	_gridColumnHelper.NewSourceEntityWanted += columnGridHelperNewSourceEntityWanted;
        }

        public void SaveChanges()
        {
            discardInvalidChanges();
            Persist();
            invalidateGrid<AbsenceView>(GridType.Absence);
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
            return Resources.Absence;
        }

    	public void OnShow()
    	{
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            // This class handle its own unit of work.
        }

        public void Persist()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var repository = new AbsenceRepository(uow);

                foreach (var absenceView in _absenceViewsToBeDeleted)
                {
                    repository.Remove(absenceView.ContainedOriginalEntity);
                } 
                
                foreach (var absenceView in getSource<AbsenceView>(GridType.Absence))
                {
                    if (!absenceView.Id.HasValue)
                    {
                        repository.Add(absenceView.ContainedEntity);
                        absenceView.UpdateAfterMerge(absenceView.ContainedEntity);
                    }
                    else
                    {
                        var absence = uow.Merge(absenceView.ContainedEntity);
                        // lazy load
                        if (!LazyLoadingManager.IsInitialized(absence.CreatedBy))
                            LazyLoadingManager.Initialize(absence.CreatedBy);
                        if (!LazyLoadingManager.IsInitialized(absence.UpdatedBy))
                            LazyLoadingManager.Initialize(absence.UpdatedBy);
                        absenceView.UpdateAfterMerge(absence);
                    }
                }

                uow.PersistAll();
                foreach (var absenceView in getSource<AbsenceView>(GridType.Absence))
                {
                    absenceView.ResetAbsenceState(null, absenceView.IsTrackerDisabled);
                }
            }
        }

        private void columnGridHelperNewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<AbsenceView> e)
        {
            e.SourceEntity = createAbsence();
        }



        #region  Grouping absence

        //private void ColumnGridHelper_NewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<GroupingAbsence> e)
        //{
        //    //GroupingAbsence groupingAbsence = new GroupingAbsence("xx<New>");
        //    //groupingAbsence.Description = new Description("Default", "Default");
        //    //e.SourceEntity = groupingAbsence;
        //    e.SourceEntity = CreateGroupAbsence();
        //}

        #endregion


        enum GridType
        {
            /// <summary>
            /// Group absence grid type.
            /// </summary>
            GroupingAbsence,

            /// <summary>
            /// Absence grid type.
            /// </summary>
            Absence,
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }

        public ViewType ViewType
        {
            get { return ViewType.Absence; }
        }

		private void discardInvalidChanges()
        {
            var persistedList = getAbsenceViews();
            var workingList = getSource<AbsenceView>(GridType.Absence);
            var hasDiscarded = false;

            foreach (var absenceView in workingList)
            {
            	if (!absenceView.Id.HasValue) continue;
            	var view = absenceView;
            	var breakingAbsenceView = persistedList.FirstOrDefault(a => a.ContainedEntity.Equals(view.ContainedEntity) &&
            	                                                            a.IsTrackerDisabled != view.IsTrackerDisabled &&
            	                                                            !a.Tracker.Equals(view.Tracker));
            	if (breakingAbsenceView == null) continue;
            	hasDiscarded = true;
            	absenceView.ResetAbsenceState(breakingAbsenceView.ContainedEntity, true);
            }
			if (hasDiscarded)
				MessageDialogs.ShowInformation(this, Resources.AbsenceSaveWasInvalid, Resources.Message);
        }

        private void AbsenceControlLayout(object sender, LayoutEventArgs e)
        {
            gridControlAbsences.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
            gridControlGroupingAbsences.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}
    }
}
