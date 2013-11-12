﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
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
        private readonly List<AbsenceView> _sourceList = new List<AbsenceView>();
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

            _absenceViewsToBeDeleted = new List<AbsenceView>();

            // Loads the tracker collection
            loadTrackerCollection();
        }

        private void ButtonAddAdvAbsenceClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Sets the row index to 0 when there's no data available in the source
            setSelectedCellWhenNoSourceAvailable();
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

        private int GetNextAbsenceId()
        {
            var absenceList = _sourceList;
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
            if (!isReadyToDelete(_sourceList)) return;
        	IList<AbsenceView> itemsToDelete =
        		_gridColumnHelper.FindSelectedItems();

        	foreach (var absenceView in itemsToDelete)
        	{
        		if (absenceView.Id.HasValue)
        			_absenceViewsToBeDeleted.Add(absenceView); // Only add items to deleted list if they exist in db.

        		_sourceList.Remove(absenceView);
        	}

        	invalidateGrid();
        }

        private void invalidateGrid()
        {
            // Gets the data source
            gridControlAbsences.RowCount = _sourceList.Count;
        	gridControlAbsences.Invalidate();
        }

		private bool isReadyToDelete(ICollection<AbsenceView> source)
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

        private static bool isDataAvailable(ICollection<AbsenceView> source)
        {
            return (source != null) && (source.Count > EmptySourceCount);
        }

        private ReadOnlyCollection<SFGridColumnBase<AbsenceView>> configureGrid()
        {
            // Holds he column list for the grid control
            IList<SFGridColumnBase<AbsenceView>> gridColumns = new List<SFGridColumnBase<AbsenceView>>();
            // Gets the relevant grid control
            
            // Adds the cell models to the grid control
            addCellmodels(gridControlAbsences);
            // Set the header count for the grid control
            gridControlAbsences.Rows.HeaderCount = EmptyHeaderCount;
            // Adds the header column for the grid control
            gridColumns.Add(new SFGridRowHeaderColumn<AbsenceView>(string.Empty));

            createColumnsForAbsenceGrid(gridColumns);

			// append audit columns
			gridColumns.Add(new SFGridReadOnlyTextColumn<AbsenceView>("CreatedBy", Resources.CreatedBy));
			gridColumns.Add(new SFGridReadOnlyTextColumn<AbsenceView>("CreatedTimeInUserPerspective", Resources.CreatedOn));
			gridColumns.Add(new SFGridReadOnlyTextColumn<AbsenceView>("UpdatedBy", Resources.UpdatedBy));
			gridColumns.Add(new SFGridReadOnlyTextColumn<AbsenceView>("UpdatedTimeInUserPerspective", Resources.UpdatedOn));

            gridControlAbsences.RowCount = gridRowCount();
            gridControlAbsences.ColCount = (gridColumns.Count - ColumnListCountMappingValue);

            return new ReadOnlyCollection<SFGridColumnBase<AbsenceView>>(gridColumns);
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

        private void createColumnsForAbsenceGrid(ICollection<SFGridColumnBase<AbsenceView>> gridColumns)
        {
            gridColumns.Add(new SFGridDescriptionNameColumn<AbsenceView>("Description", Resources.Name));
            var shortName = new SFGridDescriptionShortNameColumn<AbsenceView>("Description", Resources.ShortName, 150, false, 2)
                            	{AllowEmptyValue = true};
        	gridColumns.Add(shortName);
            gridColumns.Add(new SFGridColorPickerColumn<AbsenceView>("DisplayColor", Resources.Color));

            gridColumns.Add(new SFGridCheckBoxColumn<AbsenceView>("InContractTime", Resources.IsContractTime));
            gridColumns.Add(new SFGridCheckBoxColumn<AbsenceView>("InWorkTime", Resources.IsWorkTime));
            gridColumns.Add(new SFGridCheckBoxColumn<AbsenceView>("InPaidTime", Resources.IsPaidTime));
            if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
            {
                var payrollColumn = new SFGridEditableTextColumn<AbsenceView>("PayrollCode", 20, Resources.PayrollCode) { AllowEmptyValue = true };
                gridColumns.Add(payrollColumn);
            }
            gridColumns.Add(new SFGridByteCellColumn<AbsenceView>("Priority", Resources.Priority));
            gridColumns.Add(new SFGridCheckBoxColumn<AbsenceView>("Requestable", Resources.UseForRequests));
            var trackerColumn = new SFGridDropDownColumn<AbsenceView, TrackerView>("Tracker", Resources.TrackerTypeHeader, _trackerAdopterCollection,
                                                      "Description", typeof(TrackerView))
                                	{UseDisablePropertyCheck = true};
        	gridColumns.Add(trackerColumn);
            gridColumns.Add(new SFGridCheckBoxColumn<AbsenceView>("Confidential", Resources.Confidential));
        }

        private int gridRowCount()
        {
            var sourceListCount = _sourceList.Count;
            var gridHeaderCount = gridControlAbsences.Rows.HeaderCount;

            return (sourceListCount + gridHeaderCount);
        }

        private void loadSourceList()
        {
            _sourceList.AddRange(getAbsenceViews());
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

        private void setSelectedCellWhenNoSourceAvailable()
        {
            if (isDataAvailable(_sourceList)) return;
        	gridControlAbsences.CurrentCell.MoveTo(DefaultCellRowIndex, DefaultCellColumnIndex);
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
            toolTip1.SetToolTip(buttonAdvDeleteAbsence, Resources.Delete);
            toolTip1.SetToolTip(buttonAdvAddAbsence, Resources.AddAbsence);
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

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            gridControlAbsences.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlAbsences.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
			  
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
        {
            loadSourceList();

            var absenceColumns = configureGrid();

            _gridColumnHelper = new SFGridColumnGridHelper<AbsenceView>(gridControlAbsences,
                               absenceColumns,
                               _sourceList) {AllowExtendedCopyPaste = true};

        	_gridColumnHelper.NewSourceEntityWanted += columnGridHelperNewSourceEntityWanted;
        }

        public void SaveChanges()
        {
            discardInvalidChanges();
            Persist();
            invalidateGrid();
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
                
                foreach (var absenceView in _sourceList)
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
                foreach (var absenceView in _sourceList)
                {
                    absenceView.ResetAbsenceState(null, absenceView.IsTrackerDisabled);
                }
            }
        }

        private void columnGridHelperNewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<AbsenceView> e)
        {
            e.SourceEntity = createAbsence();
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
            var workingList = _sourceList;
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
		}
    }
}
