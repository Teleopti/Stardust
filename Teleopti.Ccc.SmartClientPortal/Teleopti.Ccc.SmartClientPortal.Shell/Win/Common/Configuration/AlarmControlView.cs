using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    class AlarmControlView : ViewBase<AlarmControlPresenter>, IDisposable, WinCode.Common.Configuration.IAlarmControlView

    {

    private readonly GridControl _grid;
        public AlarmControlView(GridControl grid)
            : base(null)
        {
            _grid = grid;
            if (!_grid.CellModels.ContainsKey("ColorPickerCell"))
                _grid.CellModels.Add("ColorPickerCell", new ColorPickerCellModel(_grid.Model));
			if (!_grid.CellModels.ContainsKey("NumericCell"))
				_grid.CellModels.Add("NumericCell", new NumericCellModel(_grid.Model));
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
                _grid.CellClick += Presenter.CellClick;
            else
                _grid.CellClick -= Presenter.CellClick;

            if (isAdd)
                _grid.SaveCellInfo += Presenter.SaveCellInfo;
            else
                _grid.SaveCellInfo -= Presenter.SaveCellInfo;
        }

        public void LoadGrid()
        {
            _grid.Refresh();
            _grid.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, _grid.ColCount));
        }


	    public void RefreshRow(int rowIndex)
	    {
		    _grid.RefreshRange(GridRangeInfo.Row(rowIndex));
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
        }
        #endregion

    	public void ShowThisItem(int alarmid)
        {
        	var handler = PresentThisItem;
            if(handler!= null)
            {          
                handler.Invoke(this,new CustomEventArgs<int>(alarmid ));
            }
        }

        public event EventHandler<CustomEventArgs<int>> PresentThisItem;

    	public event EventHandler<CustomEventArgs<string>> WarnOfThis;
		public void Warning(string message)
		{
			var handler = WarnOfThis;
			if (handler != null)
			{
				handler.Invoke(this, new CustomEventArgs<string>(message));
			}
		}
    }
}
