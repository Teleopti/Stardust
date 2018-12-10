using System;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public partial class VisualizeGridView : BaseUserControl
    {
        private readonly GridConstructor _gridConstructor;

        public VisualizeGridView()
        {
            InitializeComponent();
        }

        public VisualizeGridView(IExplorerPresenter presenter, IEventAggregator eventAggregator)
            : base(presenter)
        {
            InitializeComponent();

            _gridConstructor = new GridConstructor(eventAggregator);
            _gridConstructor.GridViewChanging += gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructor_GridViewChanged;
            _gridConstructor.FlushCache();
            _gridConstructor.BuildGridView(ShiftCreatorViewType.VisualizingGrid,
                                           ExplorerPresenter.VisualizePresenter);
            _gridConstructor.View.Grid.ContextMenuStrip = contextMenuStrip1;
            toolStripMenuItemCopyToScheduler.Enabled = false;
            if(!DesignMode)
                SetTexts();
        }

        /// <summary>
        /// Handles the GridViewChanged event of the gridConstructor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void gridConstructor_GridViewChanged(object sender, EventArgs e)
        {
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
                view.Grid.MouseDown += new MouseEventHandler(gridWorkShifts_MouseDown);
            }

            Controls.Add(view.Grid);
            view.Grid.Dock = DockStyle.Fill;
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
            toolStripMenuItemCopyToScheduler.Enabled = true;
        }

        void gridWorkShifts_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right)
                return;

            int rowIndex, colIndex;
            _gridConstructor.View.Grid.PointToRowCol(e.Location, out rowIndex, out colIndex);

            if (rowIndex < 1) 
                return;

            _gridConstructor.View.Grid.Selections.SelectRange(_gridConstructor.View.Grid.PointToRangeInfo(e.Location),
                                                              false);

            _gridConstructor.View.Grid.CurrentCell.MoveTo(rowIndex, colIndex);
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

        public override void RefreshView()
        {
            _gridConstructor.View.RefreshView();
        }

        #region IVisualizeView Members

        /// <summary>
        /// Gets the first width of the column.
        /// </summary>
        /// <returns></returns>
        public int GetFirstColumnWidth()
        {
            return 40;
        }

        #endregion

        private void toolStripMenuItemCopyToScheduler_Click(object sender, EventArgs e)
        {
            GridCurrentCellInfo info = _gridConstructor.View.Grid.CurrentCellInfo;
            if (info != null)
                ExplorerPresenter.VisualizePresenter.CopyWorkShiftToSessionDataClip(info.RowIndex);
        }
    }
}
