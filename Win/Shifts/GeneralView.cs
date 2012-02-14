﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
    /// <summary>
    /// General view
    /// </summary>
    public partial class GeneralView : BaseUserControl, IGeneralView
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly GridConstructor _gridConstructor;

        //private ShiftCreatorViewType _currentGridView;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralView"/> class.
        /// </summary>
        public GeneralView()
        {
            InitializeComponent();
        }

        public GeneralView(IExplorerPresenter explorerPresenter, IEventAggregator eventAggregator)
            : base(explorerPresenter)
        {
            _eventAggregator = eventAggregator;
            InitializeComponent();

            _gridConstructor = new GridConstructor(_eventAggregator);
            _gridConstructor.GridViewChanging += gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructor_GridViewChanged;
            _gridConstructor.FlushCache();
            _gridConstructor.BuildGridView(ShiftCreatorViewType.General, 
                                           ExplorerPresenter.GeneralPresenter.GeneralTemplatePresenter);
            //_currentGridView = ShiftCreatorViewType.General;

        }

        /// <summary>
        /// Handles the GridViewChanged event of the gridConstructor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void gridConstructor_GridViewChanged(object sender, EventArgs e)
        {
            InParameter.NotNull("sender", sender);
            GridViewBase view = (GridViewBase)sender;
            if (!_gridConstructor.IsCached)
            {
                view.Grid.ResetVolatileData();

                view.Grid.QueryCellInfo += (gridWorkShifts_QueryCellInfo);
                view.Grid.SaveCellInfo += (gridWorkShifts_SaveCellInfo);
                view.Grid.ClipboardPaste += (gridWorkShifts_ClipboardPaste);
                view.Grid.SelectionChanged += (gridWorkShifts_SelectionChanged);
                view.Grid.KeyUp += (gridWorkShifts_KeyUp);
                view.Grid.KeyDown += (gridWorkShifts_KeyDown);
            }

            Controls.Add(view.Grid);
            view.Grid.Dock = DockStyle.Fill;

            //TODO: Put this inside the explorerView with the grid control passed as a param
            /* 
            view.Grid.Width = splitContainerAdvHorizontal.Panel2.Width;
            view.Grid.VScrollBehavior = GridScrollbarMode.Enabled;
            toolStripButtonGeneral.Checked = (sender is GeneralTemplateGrid);
            toolStripButtonLimitations.Checked = (sender is ActivityTimeLimiterGrid);
            toolStripButtonDateExclusion.Checked = (sender is AccessibilityDateGrid);
            toolStripButtonWeekdayExclusion.Checked = (sender is DaysOfWeekGrid);
            toolStripButtonCombined.Checked = (sender is CombinedGrid);
             * */
        }

        #region GridControl Events

        /// <summary>
        /// Handles the GridViewChanging event of the gridConstructor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void gridConstructor_GridViewChanging(object sender, EventArgs e)
        {
            Controls.Clear();
        }

        /// <summary>
        /// Handles the QueryCellInfo event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            _gridConstructor.View.QueryCellInfo(e);
        }

        /// <summary>
        /// Handles the SaveCellInfo event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            try
            {
                _gridConstructor.View.SaveCellInfo(sender, e);
            }
            catch (TargetInvocationException)
            {
            }
        }

        /// <summary>
        /// Handles the ClipboardPaste event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCutPasteEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            _gridConstructor.View.ClipboardPaste(e);
            e.Handled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSelectionChangedEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            _gridConstructor.View.SelectionChanged(e);
        }

        /// <summary>
        /// Handles the KeyUp event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_KeyUp(object sender, KeyEventArgs e)
        {
            _gridConstructor.View.KeyUp(e);
        }

        /// <summary>
        /// Handles the KeyDown event of the gridWorkShifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void gridWorkShifts_KeyDown(object sender, KeyEventArgs e)
        {
            _gridConstructor.View.KeyDown(e);
        }

        #endregion

        #region IGeneralView Members

        public void ChangeGridView(ShiftCreatorViewType viewType)
        {
            //_currentGridView = viewType;
            if (_gridConstructor.View.Type != viewType)
            {
                switch(viewType)
                {
                    case ShiftCreatorViewType.General:
                        _gridConstructor.BuildGridView(viewType, ExplorerPresenter.GeneralPresenter.GeneralTemplatePresenter);
                        Name = "GeneralGrid";
                        break;
                    case ShiftCreatorViewType.Activities:
                        _gridConstructor.BuildGridView(viewType, ExplorerPresenter.GeneralPresenter.ActivityPresenter);
                        Name = "ActivityGrid";
                        break;
                    case ShiftCreatorViewType.DateExclusion:
                        _gridConstructor.BuildGridView(viewType, ExplorerPresenter.GeneralPresenter.AccessibilityDatePresenter);
                        Name = "AccessibilityDateGrid";
                        break;
                    case ShiftCreatorViewType.Limitation:
                        _gridConstructor.BuildGridView(viewType, ExplorerPresenter.GeneralPresenter.ActivityTimeLimiterPresenter);
                        Name = "ActivityTimeLimiterGrid";
                        break;
                    case ShiftCreatorViewType.WeekdayExclusion:
                        _gridConstructor.BuildGridView(viewType, ExplorerPresenter.GeneralPresenter.DaysOfWeekPresenter);
                        Name = "DaysOfWeekGrid";
                        break;
                }              
                _gridConstructor.View.RefreshView();
            }
            ExplorerView.SetupHelpContextForGrid(_gridConstructor.View.Grid);
        }

        public void ReflectEnteredValues()
        {
            int index = (_gridConstructor.View.Grid.Model.CurrentCellInfo == null) ? 1 :
                         _gridConstructor.View.Grid.Model.CurrentCellInfo.RowIndex;
            _gridConstructor.View.Grid.CurrentCell.MoveTo(index, 1);
            _gridConstructor.View.Grid.Selections.Clear(false);
           
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the current operation context.
        /// </summary>
        /// <returns></returns>
        private ICommonOperation GetCurrentOperationContext()
        {
            ICommonOperation context = _gridConstructor.View;
            return context;
        }

        #endregion

        /// <summary>
        /// Adds the new.
        /// </summary>
        public override void Add()
        {
            if (ExplorerView.CheckForSelectedRuleSet())
                GetCurrentOperationContext().Add();
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public override void Delete()
        {
            if(ExplorerView.AskForDelete())
                GetCurrentOperationContext().Delete();
        }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        public override void Cut()
        {
            GetCurrentOperationContext().Cut();
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public override void Copy()
        {
            GetCurrentOperationContext().Copy();
        }

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public override void Paste()
        {
            GetCurrentOperationContext().Paste();
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        /// <param name="mode"></param>
        public override void Sort(SortingMode mode)
        {
            GetCurrentOperationContext().Sort(mode);
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public override void RefreshView()
        {
            GetCurrentOperationContext().RefreshView();
        }

        private void GeneralView_Enter(object sender, EventArgs e)
        {
            Trace.WriteLine(_gridConstructor.View.Type.ToString());
            ExplorerPresenter.Model.SetSelectedView(_gridConstructor.View.Type);
        }

        public override void Amounts(IList<int> shiftAmount)
        {
            GetCurrentOperationContext().Amounts(shiftAmount);
        }
    }
}
