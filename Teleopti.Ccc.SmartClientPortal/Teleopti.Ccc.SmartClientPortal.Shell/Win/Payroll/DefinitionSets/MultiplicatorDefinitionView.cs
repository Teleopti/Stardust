using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.DefinitionSets
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MultiplicatorDefinitionView : PayrollGridViewBase, IMultiplicatorDefinitionView
    {
        private ReadOnlyCollection<SFGridColumnBase<IMultiplicatorDefinitionViewModel>> _gridColumns;
        private SFGridColumnGridHelper<IMultiplicatorDefinitionViewModel> _gridHelper;
        private IList<DayOfWeekAdapter> _dayOfWeekAdapterCollection;
        private IList<IMultiplicatorDefinitionAdapter> _multiplicatorDefinitionTypeCollection;
        private Point _gridPoint;
        private IList<IMultiplicatorDefinitionViewModel> _inMemoryViewModelCollection = new List<IMultiplicatorDefinitionViewModel>();
        private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();

        public MultiplicatorDefinitionView(IExplorerView explorerView)
            : base(explorerView)
        {
            InitializeComponent();
            prepareContextMenuStrip();
            ExplorerView.ExplorerPresenter.Model.MultiplicatorCollection = ExplorerView.ExplorerPresenter.Helper.LoadMultiplicatorList();
            loadMultiplicatorDefinitionTypeCollection();
            loadDayOfWeekAdapterCollection();
            ConfigureGrid();
            RegisterForMessageBrokerEvents(typeof(IMultiplicator));
            OnEventMessageHandlerChanged += (MultiplicatorDefinitionView_OnEventMessageHandlerChanged);
        }

        void MultiplicatorDefinitionView_OnEventMessageHandlerChanged(object sender, EventMessageArgs e)
        {
            HandleMessageBroker(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ConfigureGrid()
        {
            IList<SFGridColumnBase<IMultiplicatorDefinitionViewModel>> columnList = new List<SFGridColumnBase<IMultiplicatorDefinitionViewModel>>();

            // Grid must have a Header column
            columnList.Add(new SFGridRowHeaderColumn<IMultiplicatorDefinitionViewModel>(string.Empty));

            // Add cellmodels
            gridControlMultiplicatorDefinition.CellModels.Add("DateTimeCellModel", new DateTimeCellModel(gridControlMultiplicatorDefinition.Model));
            gridControlMultiplicatorDefinition.CellModels.Add("HourMinutes", new TimeSpanTimeOfDayCellModel(gridControlMultiplicatorDefinition.Model));
            gridControlMultiplicatorDefinition.CellModels.Add("IgnoreCell", new IgnoreCellModel(gridControlMultiplicatorDefinition.Model));

            var sfGridDropDownColumn = new SFGridDropDownColumn<IMultiplicatorDefinitionViewModel, IMultiplicatorDefinitionAdapter>(
                    "MultiplicatorDefinitionType", Resources.Type, _multiplicatorDefinitionTypeCollection, "Name", typeof(MultiplicatorDefinitionAdapter));

            columnList.Add(sfGridDropDownColumn);
            columnList.Add(new SFGridDropDownEnumColumn<IMultiplicatorDefinitionViewModel, DayOfWeekAdapter, DayOfWeek>("DayOfWeek", Resources.DayOfWeek, _dayOfWeekAdapterCollection, "DisplayName", "DayOfWeek"));
            columnList.Add(new SFGridHourMinutesColumn<IMultiplicatorDefinitionViewModel>("StartTime", Resources.StartTime));
            columnList.Add(new SFGridHourMinutesColumn<IMultiplicatorDefinitionViewModel>("EndTime", Resources.EndTime));
            columnList.Add(new SFGridDateTimeColumn<IMultiplicatorDefinitionViewModel>("FromDate", Resources.From, 100));
            columnList.Add(new SFGridDateTimeColumn<IMultiplicatorDefinitionViewModel>("ToDate", Resources.To, 100));
            columnList.Add(new SFGridDynamicDropDownColumn<IMultiplicatorDefinitionViewModel, IMultiplicator>("Multiplicator", Resources.Multiplicator, null, "MultiplicatorCollection", "Description", typeof(Multiplicator)));

            // Adds column list.
            _gridColumns = new ReadOnlyCollection<SFGridColumnBase<IMultiplicatorDefinitionViewModel>>(columnList);

            ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.LoadMultiplicatorDefinitions();
            
            _gridHelper = new SFGridColumnGridHelper<IMultiplicatorDefinitionViewModel>(gridControlMultiplicatorDefinition,
                                                                                        _gridColumns, ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.ModelCollection, false);
            _gridHelper.UnbindClipboardPasteEvent();

            _gridHelper.AllowExtendedCopyPaste = true;
            gridControlMultiplicatorDefinition.CellsChanged += (gridControlDateViewGridCellChanged);
            gridControlMultiplicatorDefinition.CurrentCellCloseDropDown += (gridControlMultiplicatorDefinitionCurrentCellCloseDropDown);
            gridControlMultiplicatorDefinition.MouseDown += (gridControlMultiplicatorDefinitionMouseDown);
            gridControlMultiplicatorDefinition.ClipboardPaste += (gridControlMultiplicatorDefinitionClipboardPaste);
            gridControlMultiplicatorDefinition.ClipboardCut += (gridControlMultiplicatorDefinitionClipboardCut);
            gridControlMultiplicatorDefinition.ClipboardCopy += (gridControlMultiplicatorDefinitionClipboardCopy);
            gridControlMultiplicatorDefinition.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            
        }

        void gridControlMultiplicatorDefinitionClipboardCopy(object sender, GridCutPasteEventArgs e)
        {
            _inMemoryViewModelCollection = _gridHelper.FindSelectedItems();
        }

        void toolStripCutClick(object sender, EventArgs e)
        {
            GridRangeInfoList gridRangeInfoList = gridControlMultiplicatorDefinition.Selections.Ranges;
            gridControlMultiplicatorDefinitionClipboardCut(gridControlMultiplicatorDefinition, new GridCutPasteEventArgs(true,false,0,gridRangeInfoList));
        }

        void toolStripPasteClick(object sender, EventArgs e)
        {
            GridRangeInfoList gridRangeInfoList = gridControlMultiplicatorDefinition.Selections.Ranges;
            gridControlMultiplicatorDefinitionClipboardPaste(gridControlMultiplicatorDefinition, new GridCutPasteEventArgs(true, false, 0, gridRangeInfoList));
        }

        void toolStripCopyClick(object sender, EventArgs e)
        {
            GridRangeInfoList gridRangeInfoList = gridControlMultiplicatorDefinition.Selections.Ranges;
            gridControlMultiplicatorDefinitionClipboardCopy(gridControlMultiplicatorDefinition, new GridCutPasteEventArgs(true, false, 0, gridRangeInfoList));
        }

        void gridControlMultiplicatorDefinitionClipboardCut(object sender, GridCutPasteEventArgs e)
        {
            if(!isHeader(e))
                return;

            _inMemoryViewModelCollection = _gridHelper.FindSelectedItems();

            if (_inMemoryViewModelCollection.Count < 1)
                return;

            // Build a string for for all viewModels in range and insert into clipboard.
            _externalExceptionHandler.AttemptToUseExternalResource(()=>
            Clipboard.SetText(ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.BuildCopyString(_inMemoryViewModelCollection)));

            // Delete selected rows.
            deleteSelectedItems();
            e.Handled = true;
        }

        private bool isHeader(GridCutPasteEventArgs e)
        {
            //This is so stupid?!?!? Just to get rid of cut in the header
            foreach (GridRangeInfo range in e.RangeList)
            {
                if (range.Top == gridControlMultiplicatorDefinition.Rows.HeaderCount)
                {
                    e.Handled = true;
                    return false;
                }
            }
            return true;
        }

        private bool isRowsCopied(ClipHandler<string> clipHandler)
        {
            int columnsInGrid = gridControlMultiplicatorDefinition.ColCount - gridControlMultiplicatorDefinition.Rows.HeaderCount;
            if (columnsInGrid != clipHandler.ColSpan())
                return false;
            
            return true;
        }
        
        private void gridControlMultiplicatorDefinitionClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            handleRowPaste();
            e.Handled = true;
            gridControlMultiplicatorDefinition.Invalidate();
            invokeGridDataChanged();
        }

        private void handleRowPaste()
        {
            ClipHandler<string> clipHandler = GridHelper.ConvertClipboardToClipHandlerString();

            if (!isRowsCopied(clipHandler))
            {
                // Cell is copied - use gridhelpers clipboard handling
                _gridHelper.PasteFromClipboard();
            }
            else
            {
                // Row is copied - handle paste here
                GridRangeInfoList selectedRows = gridControlMultiplicatorDefinition.Selections.GetSelectedRows(true, true);
                int copiedRowsCount = _inMemoryViewModelCollection.Count;
                int selectedRowsCount = (selectedRows.ActiveRange.Bottom - selectedRows.ActiveRange.Top) + 1;
                int rowsToPasteCount = getNumberOfRowsToPaste(selectedRows.ActiveRange, copiedRowsCount, selectedRowsCount);
                int pasteRowOffsetIndex = 0;
                int clipIndex = 0;

                for (int i = 1; i <= rowsToPasteCount; i++)
                {
                    int currentRowIndex = selectedRows.ActiveRange.Top + pasteRowOffsetIndex;
                    IMultiplicatorDefinitionViewModel viewModel = _gridHelper.GetItemForRow(currentRowIndex);
                    if (viewModel == null)
                        break; // No row exist to paste onto

                    // Do replace of selected row with new item using array of strings of column/property data to be resolved
                    replaceDefinitionWithNew(viewModel, _inMemoryViewModelCollection[clipIndex]);

                    pasteRowOffsetIndex++;
                    clipIndex++;

                    if (clipIndex == copiedRowsCount)
                        clipIndex = 0; // Prepare to lay out copied rows for a new round
                }
            }
        }

        private int getNumberOfRowsToPaste(GridRangeInfo selectedRows, int copiedRowsCount, int selectedRowsCount)
        {
            int rowsInGrid = gridControlMultiplicatorDefinition.RowCount - gridControlMultiplicatorDefinition.Rows.HeaderCount;
            int rowsToPasteCount;
            if (((copiedRowsCount + selectedRows.Top) - 1) > rowsInGrid)
            {
                rowsToPasteCount = (rowsInGrid - selectedRows.Top) + 1;
            }
            else if (selectedRowsCount < copiedRowsCount)
            {
                rowsToPasteCount = copiedRowsCount;
            }
            else
            {
                int iterateCopiedRows = selectedRowsCount / copiedRowsCount;
                rowsToPasteCount = iterateCopiedRows * copiedRowsCount;
            }
            return rowsToPasteCount;
        }

        private void gridControlDateViewGridCellChanged(object sender, GridCellsChangedEventArgs e)
        {
            invokeGridDataChanged();
        }

        void gridControlMultiplicatorDefinitionMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _gridPoint = e.Location;
                IMultiplicatorDefinitionViewModel rightClickedItem = _gridHelper.FindItemByPoint(_gridPoint);
                EnableContextMenu(false);
                if (rightClickedItem != null)
                {
                    EnableContextMenu(true);
                    var selectedItems = _gridHelper.FindSelectedItems();

                    if (!selectedItems.Contains(rightClickedItem))
                    {
                        gridControlMultiplicatorDefinition.Selections.Clear();
                        gridControlMultiplicatorDefinition.CurrentCell.Deactivate(false);
                        gridControlMultiplicatorDefinition.Selections.SelectRange(gridControlMultiplicatorDefinition.PointToRangeInfo(_gridPoint), true);
                    }
                }
            }
        }
        
        public void EnableContextMenu(bool value)
        {
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[2].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[3].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[4].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[5].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[6].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[7].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[8].Enabled = value;
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items[9].Enabled = value;
        }
        void gridControlMultiplicatorDefinitionCurrentCellCloseDropDown(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
        {
            var gridBase = (GridControl)sender;
            if (gridBase != null)
            {
                if (gridBase.CurrentCell.RowIndex > gridBase.CurrentCell.Model.Grid.Rows.HeaderCount && gridBase.CurrentCell.ColIndex == 1)
                {
                    var chosenMultiplicatorDefinitionAdapter = (MultiplicatorDefinitionAdapter)gridBase.CurrentCellInfo.CellView.ControlValue;

                    // Gets the current item.
                    int offset = gridBase.CurrentCell.RowIndex - (gridBase.CurrentCell.Model.Grid.Rows.HeaderCount + 1);
                    IMultiplicatorDefinitionViewModel multiplicatorDefinitionViewModel = ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.ModelCollection[offset];
                    if (chosenMultiplicatorDefinitionAdapter.MultiplicatorDefinitionType != multiplicatorDefinitionViewModel.MultiplicatorDefinitionType.MultiplicatorDefinitionType)
                    {
                        // Replace selected item with a new definition of different type
                        replaceDefinitionWithNew(_gridHelper.FindSelectedItem(), chosenMultiplicatorDefinitionAdapter.MultiplicatorDefinitionType);
                        gridControlMultiplicatorDefinition.Invalidate();
                        invokeGridDataChanged();
                    }
                }
            }
        }

        private void replaceDefinitionWithNew(IMultiplicatorDefinitionViewModel viewModel, Type definitionType)
        {
            foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
            {
                int orderIndex = ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.DeleteSelected(definitionSet, viewModel.DomainEntity);

                //hmmm...
                if (definitionType == typeof(DateTimeMultiplicatorDefinition))
                {
                    ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.AddNewDateTimeAt(definitionSet, orderIndex, viewModel.Multiplicator);
                }
                else
                {
                    ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.AddNewDayOfWeekAt(definitionSet, orderIndex, viewModel.Multiplicator);
                }
            }
        }

        private void replaceDefinitionWithNew(IMultiplicatorDefinitionViewModel viewModelToDelete, IMultiplicatorDefinitionViewModel viewModelToCopy)
        {
            foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
            {
                int orderIndex = ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.DeleteSelected(definitionSet, viewModelToDelete.DomainEntity);
                ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.AddNewAt(definitionSet, viewModelToCopy, orderIndex, _multiplicatorDefinitionTypeCollection);
            }
        }

        public event EventHandler GridDataChanged;

        private void loadMultiplicatorDefinitionTypeCollection()
        {
            _multiplicatorDefinitionTypeCollection = new List<IMultiplicatorDefinitionAdapter>
            {
                new MultiplicatorDefinitionAdapter(typeof (DateTimeMultiplicatorDefinition), Resources.FromTo),
                new MultiplicatorDefinitionAdapter(typeof (DayOfWeekMultiplicatorDefinition), Resources.DayOfWeekCapital)
            };
        }

        private void loadDayOfWeekAdapterCollection()
        {
            _dayOfWeekAdapterCollection = new List<DayOfWeekAdapter>();
            IList<DayOfWeek> dayOfWeekCollection = DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture);
            
            foreach (DayOfWeek dayOfWeek in dayOfWeekCollection)
            {
                var multiplicatorTypeView = new DayOfWeekAdapter(dayOfWeek);
                _dayOfWeekAdapterCollection.Add(multiplicatorTypeView);
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Reload()
        {
            ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.LoadMultiplicatorDefinitions();
            gridControlMultiplicatorDefinition.RowCount = _gridHelper.SourceList.Count;
            gridControlMultiplicatorDefinition.Invalidate();
        }

        /// <summary>
        /// Invokes the grid data changed.
        /// </summary>
        private void invokeGridDataChanged()
        {
        	var handler = GridDataChanged;
            if (handler != null)
            {
                handler.Invoke(this, new EventArgs());
            }
            gridControlMultiplicatorDefinition.Refresh();
        }

        /// <summary>
        /// Prepares the context menu strip.
        /// </summary>
        private void prepareContextMenuStrip()
        {
            gridControlMultiplicatorDefinition.ContextMenuStrip = new ContextMenuStrip();

            var toolStripAddDateTime = new ToolStripMenuItem(Resources.AddDateTime);
            toolStripAddDateTime.Click += delegate { addNewDateTime(); };
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripAddDateTime);

            var toolStripAddDayOfWeek = new ToolStripMenuItem(Resources.AddDayOfWeek);
            toolStripAddDayOfWeek.Click += delegate { addNewDayOfWeek(); };
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripAddDayOfWeek);

            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            var toolStripDelete = new ToolStripMenuItem(Resources.Delete);
            toolStripDelete.Click += delegate { DeleteSelected(); };
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripDelete);

            var toolStripMoveUp = new ToolStripMenuItem(Resources.MoveUp);
            toolStripMoveUp.Click += delegate { MoveUp(); };
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripMoveUp);

            var toolStripMoveDown = new ToolStripMenuItem(Resources.MoveDown);
            toolStripMoveDown.Click += delegate { MoveDown(); };
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripMoveDown);

            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            var toolStripCut = new ToolStripMenuItem(Resources.Cut);
            toolStripCut.Click += (toolStripCutClick);
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripCut);

            var toolStripCopy = new ToolStripMenuItem(Resources.Copy);
            toolStripCopy.Click += (toolStripCopyClick);
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripCopy);

            var toolStripPaste = new ToolStripMenuItem(Resources.Paste);
            toolStripPaste.Click += (toolStripPasteClick);
            gridControlMultiplicatorDefinition.ContextMenuStrip.Items.Add(toolStripPaste);

        }

        public override void DeleteSelected()
        {
            if (IsReadyToDelete(_gridHelper))
            {
                deleteSelectedItems();
            }
        }

        private void deleteSelectedItems()
        {
            IList<IMultiplicatorDefinitionViewModel> itemsToDelete = _gridHelper.FindSelectedItems();

            foreach (IMultiplicatorDefinitionViewModel definitionViewModel in itemsToDelete)
            {
                foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
                {
                    ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.DeleteSelected(definitionSet, definitionViewModel.DomainEntity);
                }

            }
            gridControlMultiplicatorDefinition.Selections.Clear();
            gridControlMultiplicatorDefinition.RowCount = _gridHelper.SourceList.Count;
            gridControlMultiplicatorDefinition.Invalidate();
            invokeGridDataChanged();
        }

        public override void MoveUp()
        {
            GridRangeInfo activeRange = gridControlMultiplicatorDefinition.Selections.Ranges.ActiveRange;

            if (activeRange.Top > gridControlMultiplicatorDefinition.Rows.HeaderCount + 1)
            {
                ReadOnlyCollection<IMultiplicatorDefinitionViewModel> selectedItems = _gridHelper.FindItemsBySelectionOrPoint(_gridPoint);
                gridControlMultiplicatorDefinition.Selections.Clear();
                foreach (IMultiplicatorDefinitionViewModel definitionViewModel in selectedItems)
                {
                    if (ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection != null)
                    {
                        foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
                        {
                            ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.MoveUp(definitionSet, definitionViewModel.DomainEntity);
                        }
                    }
                }
                gridControlMultiplicatorDefinition.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(-1, 0));


                gridControlMultiplicatorDefinition.Invalidate();
                invokeGridDataChanged();
            }
        }

        public override void MoveDown()
        {
            GridRangeInfo activeRange = gridControlMultiplicatorDefinition.Selections.Ranges.ActiveRange;
            if (activeRange.Bottom < gridControlMultiplicatorDefinition.RowCount)
            {
                ReadOnlyCollection<IMultiplicatorDefinitionViewModel> selectedItems = _gridHelper.FindItemsBySelectionOrPoint(_gridPoint);
                gridControlMultiplicatorDefinition.Selections.Clear();
                foreach (IMultiplicatorDefinitionViewModel definitionViewModel in selectedItems.Reverse())
                {
                    if (ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection != null)
                    {
                        foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
                        {
                            ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.MoveDown(definitionSet, definitionViewModel.DomainEntity);
                        }
                    }
                }
                gridControlMultiplicatorDefinition.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(1, 0));
                gridControlMultiplicatorDefinition.Invalidate();
                invokeGridDataChanged();
            }
        }

        private void addNewDateTime()
        {
            if (ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection != null)
            {
                foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
                {
                    if (ExplorerView.ExplorerPresenter.Model.MultiplicatorCollection.Count(p => p.MultiplicatorType == definitionSet.MultiplicatorType) <= 0)
                    {
                        ShowErrorMessage(string.Format(CurrentUICulture, Resources.NoMultiplicators, LanguageResourceHelper.TranslateEnumValue(definitionSet.MultiplicatorType)), Resources.ErrorMessage);
                        return;
                    }

                    ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.AddNewDateTime(definitionSet);
                    gridControlMultiplicatorDefinition.RowCount = ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.ModelCollection.Count;
                }
                invokeGridDataChanged();
            }
        }

        private void addNewDayOfWeek()
        {
            if (ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection != null)
            {
                foreach (IMultiplicatorDefinitionSet definitionSet in ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection)
                {
                    if (ExplorerView.ExplorerPresenter.Model.MultiplicatorCollection.Count(p => p.MultiplicatorType == definitionSet.MultiplicatorType) <= 0)
                    {
                        ShowErrorMessage(string.Format(CurrentUICulture, Resources.NoMultiplicators, LanguageResourceHelper.TranslateEnumValue(definitionSet.MultiplicatorType)), Resources.ErrorMessage);
                        return;
                    }

                    ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.AddNewDayOfWeek(definitionSet);
                    gridControlMultiplicatorDefinition.RowCount = ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.ModelCollection.Count;
                }
                invokeGridDataChanged();
            }
        }

        public override void AddNew()
        {
            addNewDayOfWeek();
        }

        private void gridControlMultiplicatorDefinitionKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Delete)
            {
                DeleteSelected();
            }
        }

        public override void HandleUpdateFromMessageBroker(EventMessageArgs e)
        {
            ExplorerView.ExplorerPresenter.MultiplicatorDefinitionPresenter.UpdateMultiplicatorCollectionUponMultiplicatorChanges(e);
        }
    }
}