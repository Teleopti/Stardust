using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts.Grids;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public class GridConstructor : Component
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Dictionary<ShiftCreatorViewType, GridViewBase> _viewCache = new Dictionary<ShiftCreatorViewType, GridViewBase>();

        public GridConstructor(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void FlushCache()
        {
            if (_viewCache != null)
            {
                _viewCache.Clear();
            }
        }

        public event EventHandler GridViewChanged;

        public event EventHandler GridViewChanging;

        public void BuildGridView<T>(ShiftCreatorViewType type, T presenter)
        {
        	var handlerChanging = GridViewChanging;
			if (handlerChanging != null) handlerChanging(_view, EventArgs.Empty);

            IsCached = _viewCache.ContainsKey(type);
            if (!IsCached)
                cacheGridView(type, presenter);

            _view = _viewCache[type];
            _view.Grid.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();

        	var handlerChanged = GridViewChanged;
            if (handlerChanged != null) handlerChanged(_view, EventArgs.Empty);

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

        public bool IsCached
        {
            get; 
            private set;
        }

        private void cacheGridView<T>(ShiftCreatorViewType type, T presenter)
        {
            GridViewBase view = null;

            switch (type)
            {
                case ShiftCreatorViewType.General:
                    view = new GeneralTemplateGrid((IGeneralTemplatePresenter)presenter, new GridControl(), _eventAggregator);
                    break;
                case ShiftCreatorViewType.Activities:
                    view = new ActivitiesGrid((IActivityPresenter)presenter, new GridControl(),_eventAggregator);
                    break;
                case ShiftCreatorViewType.Limitation:
                    view = new ActivityTimeLimiterGrid((IActivityTimeLimiterPresenter)presenter, new GridControl(), _eventAggregator);
                    break;
                case ShiftCreatorViewType.WeekdayExclusion:
                    view = new DaysOfWeekGrid((IDaysOfWeekPresenter)presenter, new GridControl(), _eventAggregator);
                    break;
                case ShiftCreatorViewType.DateExclusion:
                    view = new AccessibilityDateGrid((IAccessibilityDatePresenter)presenter, new GridControl(), _eventAggregator);
                    break;
                case ShiftCreatorViewType.VisualizingGrid:
                    view = new VisualizeGrid((IVisualizePresenter)presenter, new GridControl());
                    break;
            }

            _viewCache.Add(type, view);
        }

        private GridViewBase _view;

        public GridViewBase View
        {
            get { return _view; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_view != null) _view.Dispose();
                if (_viewCache != null) _viewCache.Clear();
            }
        }
    }
}
