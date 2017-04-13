using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Intraday;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class ManageAlarmSituationView : ViewBase<ManageAlarmSituationPresenter>, IDisposable, IManageAlarmSituationView
    {
        private GridControl _grid;

        public ManageAlarmSituationView(GridControl grid)
            : base(null)
        {
            _grid = grid;
        }

        protected override void OnPresenterSet()
        {
            base.OnPresenterSet();
            SetEventHandlers(true);
        }

        /// <summary>
        /// add or remove the eventhadler
        /// </summary>
        /// <param name="isAdd">if set to <c>true</c> [is add].</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        public void SetEventHandlers(bool isAdd)
        {
            if (Presenter == null) return;//so it doesnt crash when the load is broken
            if (isAdd)
                _grid.QueryColCount += Presenter.QueryColCount;
            else
                _grid.QueryColCount -= Presenter.QueryColCount;

            if (isAdd)
                _grid.QueryRowCount += Presenter.QueryRowCount;
            else
                _grid.QueryRowCount -= Presenter.QueryRowCount;

            if (isAdd)
                _grid.QueryCellInfo += Presenter.QueryCellInfo;
            else
                _grid.QueryCellInfo -= Presenter.QueryCellInfo;

            if (isAdd)
                _grid.SaveCellInfo += Presenter.SaveCellInfo;
            else
                _grid.SaveCellInfo -= Presenter.SaveCellInfo;
        }

        public void LoadGrid()
        {
            Presenter.Load();
            _grid.Refresh();
            _grid.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, _grid.ColCount));
        }

        public void RefreshGrid()
        {
            if (_grid.InvokeRequired)
            {
                _grid.Invoke(new MethodInvoker(RefreshGrid));
            }
            else
            {
                _grid.ColCount = Presenter.ColCount;
                _grid.RowCount = Presenter.RowCount;
                _grid.InvalidateRange(GridRangeInfo.Table());
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();

            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            SetEventHandlers(false);
            Presenter.Dispose();
	        Presenter = null;
	        _grid = null;
        }
        #endregion
    }
}
