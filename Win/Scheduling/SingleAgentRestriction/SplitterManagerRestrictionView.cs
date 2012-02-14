using System;
using Syncfusion.Windows.Forms.Tools.Enums;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction
{
    public class SplitterManagerRestrictionView : IDisposable
    {
        private bool _showEditor = true;
        private bool _showGraph = true;
        private bool _showResult = true;
        private bool _showRestrictionView;
        private int _editorHeight = 120;
        private TeleoptiLessIntelligentSplitContainer _gridEditorSplitter;
        private TeleoptiLessIntelligentSplitContainer _restrictionViewSplitter;

        public TeleoptiLessIntelligentSplitContainer MainSplitter { get; set; }
        public TeleoptiLessIntelligentSplitContainer GraphResultSplitter { get; set; }

        public TeleoptiLessIntelligentSplitContainer RestrictionViewSplitter
        {
            get { return _restrictionViewSplitter; }
            set
            {
                _restrictionViewSplitter = value;
                RestrictionViewSplitter.Panel1Collapsed = true;
                RestrictionViewSplitter.SplitterWidth = 1;
                RestrictionViewSplitter.Style = Style.None;
            }
        }

        public TeleoptiLessIntelligentSplitContainer GridEditorSplitter
        {
            get { return _gridEditorSplitter; }
            set
            {
                _gridEditorSplitter = value;
                //GridEditorSplitter.IsSplitterFixed = true;
                GridEditorSplitter.SplitterDistance = GridEditorSplitter.Height - _editorHeight;
                //GridEditorSplitter.FixedPanel = FixedPanel.Panel2;
            }
        }

        public bool ShowEditor
        {
            set
            {
                _showEditor = value;
                toggleEditor();
            }

            get
            {
                return _showEditor;
            }
        }

        private void toggleEditor()
        {
            if (!_showEditor)
            {
                GridEditorSplitter.Panel2Collapsed = true;
                GridEditorSplitter.SplitterWidth = 7;
                GridEditorSplitter.Style = Style.None;
            }
            if (_showEditor)
            {
                GridEditorSplitter.Panel2Collapsed = false;
                GridEditorSplitter.Style = Style.Office2007Blue;
                GridEditorSplitter.SplitterWidth = 7;
                //GridEditorSplitter.SplitterDistance = GridEditorSplitter.Height - _editorHeight;

                int preferedMainSplitterDistance = MainSplitter.Height - _editorHeight - MainSplitter.Panel2MinSize;

                if (MainSplitter.SplitterDistance > preferedMainSplitterDistance)
                {
                    if(preferedMainSplitterDistance >= MainSplitter.Panel1MinSize)
                        MainSplitter.SplitterDistance = preferedMainSplitterDistance;
                }
            }
        }

        public bool ShowGraph
        {
            set
            {
                _showGraph = value;
                setupResultAndGraph();
            }

            get
            {
                return _showGraph;
            }
        }

        public bool ShowResult
        {
            set
            {
                _showResult = value;
                setupResultAndGraph();
            }

            get
            {
                return _showResult;
            }
        }

        public bool ShowRestrictionView
        {
            set
            {
                _showRestrictionView = value;
                toggleRestrictionView();
            }

            get
            {
                return _showRestrictionView;
            }
        }

        private void toggleRestrictionView()
        {
            if (!_showRestrictionView)
            {
                RestrictionViewSplitter.Panel1Collapsed = true;
                RestrictionViewSplitter.SplitterWidth = 7;
                RestrictionViewSplitter.Style = Style.None;
            }
            if (_showRestrictionView)
            {
                RestrictionViewSplitter.Panel1Collapsed = false;
                RestrictionViewSplitter.Style = Style.Office2007Blue;
                RestrictionViewSplitter.SplitterWidth = 7;
            }
        }

        public void ToggleEditorHeight()
        {
            if (_editorHeight == 100)
                _editorHeight = 150;
            else
            {
                _editorHeight = 100;
            }
            GridEditorSplitter.SplitterDistance = GridEditorSplitter.Height - _editorHeight;
        }

        private void setupResultAndGraph()
        {
            checkIfBothGraphAndResultHidden();

            if (_showResult && _showGraph)
            {
                GraphResultSplitter.Panel1Collapsed = false;
                GraphResultSplitter.Panel2Collapsed = false;
                return;
            }

            GraphResultSplitter.SplitterWidth = 1;
            GraphResultSplitter.Style = Style.None; 

            if (!_showGraph)
                GraphResultSplitter.Panel1Collapsed = true;

            if (!_showResult)
                GraphResultSplitter.Panel2Collapsed = true;
        }

        private void checkIfBothGraphAndResultHidden()
        {
            bool bothHidden = !_showGraph && !_showResult;

            MainSplitter.Panel1Collapsed = bothHidden;
            MainSplitter.IsSplitterFixed = bothHidden;
            GraphResultSplitter.IsSplitterFixed = bothHidden;

            if (bothHidden)
            {
                MainSplitter.SplitterWidth = 1;
                MainSplitter.Style = Style.None;
                GraphResultSplitter.SplitterWidth = 1;
                GraphResultSplitter.Style = Style.None;
            }
            else
            {
                MainSplitter.Style = Style.Office2007Blue;
                MainSplitter.SplitterWidth = 7;
                GraphResultSplitter.Style = Style.Office2007Blue;
                GraphResultSplitter.SplitterWidth = 7;
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
            _gridEditorSplitter = null;
            _restrictionViewSplitter = null;
            MainSplitter = null;
            GraphResultSplitter = null;
        }
        #endregion
    }
}