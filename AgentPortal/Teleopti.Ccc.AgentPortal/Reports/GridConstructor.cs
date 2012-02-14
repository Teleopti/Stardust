using System;
using System.Collections.Generic;
using System.ComponentModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Reports.Grid;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    public enum ViewType
    {
        MyScheduleGridView
    } ;


    internal class GridConstructor : Component
    {
        /// <summary>
        /// View Cache Dictionary
        /// </summary>
        private readonly Dictionary<ViewType, GridViewBase> _viewCache = new Dictionary<ViewType, GridViewBase>();

        public void FlushCache()
        {
            if (_viewCache != null)
            {
                _viewCache.Clear();
            }
        }

        public event EventHandler GridViewChanged;

        public event EventHandler GridViewChanging;

        public void BuildGridView(ViewType type)
        {
            if (GridViewChanging != null)
            {
                GridViewChanging(_view, new EventArgs());
            }

            IsCached = _viewCache.ContainsKey(type);

            if (!IsCached)
                CacheGridView(type);

            _view = _viewCache[type];

            if (GridViewChanged != null)
                GridViewChanged(_view, new EventArgs());

            if (!IsCached)
            {
                _view.Grid.BeginUpdate();

                _view.ClearView();
                _view.CreateHeaders();
                _view.CreateContextMenu();
                _view.PrepareView();
                _view.MergeHeaders();

                _view.Grid.EndUpdate();
            }
        }


        private bool _isCached;

        public bool IsCached
        {
            get { return _isCached; }
            set { _isCached = value; }
        }

        private void CacheGridView(ViewType type)
        {
            GridViewBase view;

            switch (type)
            {
                case ViewType.MyScheduleGridView:
                    view = new MyScheduleGrid(new GridControl());
                    break;
                default:
                    throw new InvalidEnumArgumentException("type", (int) type, typeof (ViewType));
            }
            _viewCache.Add(type, view);
        }

        /// <summary>
        /// GridViewBase instance
        /// </summary>
        private GridViewBase _view;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public GridViewBase View
        {
            get { return _view; }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_view != null)
                    _view.Dispose();

                if (_viewCache != null)
                    _viewCache.Clear();
            }
        }
    }
}