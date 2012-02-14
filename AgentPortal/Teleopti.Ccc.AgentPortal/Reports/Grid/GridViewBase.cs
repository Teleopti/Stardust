using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Reports.Grid
{
    internal abstract class GridViewBase : IDisposable
    {
        private GridControl _grid;

        protected GridViewBase(GridControl grid)
        {
            Grid = grid;
            Init();
        }

        private void Init()
        {
            // Show the grid like an Excel Worksheet.
            GridHelper.GridStyle(Grid);
            // Override the column resize style.
            Grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;

            Grid.Dock = DockStyle.Fill;
            Grid.Location = new System.Drawing.Point(0, 0);
            Grid.Visible = true;

            //Grid.CellModels.Add("HourMinutes", new TimeSpanHourMinutesCellModel(Grid.Model));
        }

        //internal abstract ViewType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public GridControl Grid
        {
            get { return _grid; }
            set { _grid = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public virtual void ClearView()
        {
            RowCount = 0;
            ColCount = 0;
            Grid.ColCount = 0;
            Grid.RowCount = 0;

            Grid.RowHeights.ResetModified();
        }


        public int ColCount
        {
            get { return Grid.ColCount; }
            set { Grid.ColCount = value; }
        }


        public int RowCount
        {
            get { return Grid.RowCount; }
            set { Grid.RowCount = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public int ColHeaders
        {
            get { return Grid.Cols.HeaderCount; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public int RowHeaders
        {
            get { return Grid.Rows.HeaderCount; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public bool IsRightToLeft
        {
            get { return Grid.RightToLeft == System.Windows.Forms.RightToLeft.Yes; }
            set
            {
                if (value) Grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
                else Grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual bool ValidCell(int columnIndex, int rowIndex)
        {
            return ValidColumn(columnIndex) && ValidRow(rowIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual bool ValidColumn(int columnIndex)
        {
            bool ret = false;
            if ((columnIndex != -1) && (ColCount > 0))
                if (columnIndex <= ColCount - 1)
                    ret = true;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual bool ValidRow(int rowIndex)
        {
            return rowIndex >= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void CreateHeaders()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        internal virtual void CreateContextMenu()
        {
            if (Grid.ContextMenuStrip != null)
            {
                Grid.ContextMenuStrip.Items.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal virtual void MergeHeaders()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void KeyUp(KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(Grid, e);
            //e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = ColCount - 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
        }


        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-22
        /// </remarks>
        internal virtual void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void SelectionChanged(GridSelectionChangedEventArgs e)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void PrepareView()
        {
            RowCount = 0;
        }

        internal virtual void CellClick(object sender, GridCellClickEventArgs e)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void AddNewGridRow<T>(object sender, T eventArgs) where T : EventArgs
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void InsertNewGridRow<T>(object sender, T eventArgs) where T : EventArgs
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        internal virtual void DeleteSelectedGridRows<T>(object sender, T eventArgs) where T : EventArgs
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}