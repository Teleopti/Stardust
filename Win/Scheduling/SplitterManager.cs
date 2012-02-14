using System;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class SplitterManager : IDisposable
    {
        private bool _showEditor = true;
        private bool _showGraph = true;
        private bool _showResult = true;
        private int _editorPositionFromBottom;
        private int _graphResultSplitterPos;
        public SplitContainerAdv MainSplitter { get; set; }
        public SplitContainerAdv GridEditorSplitter { get; set; }
        public SplitContainerAdv GraphResultSplitter { get; set; }

        public bool ShowEditor
        {
            set
            {
                SetSplitterPosition();
                _showEditor = value;
                ToggleEditor();
            }
            get
            {
                return _showEditor;
            }
        }

        private void ToggleEditor()
        {
            if (!_showEditor)
            {
                GridEditorSplitter.Panel2Collapsed = true;
                GridEditorSplitter.IsSplitterFixed = true;
            }
            //hide
            if (_showEditor)
            {
                GridEditorSplitter.IsSplitterFixed = false;
                GridEditorSplitter.Panel2Collapsed = false;

            }
            CheckIfBothGraphAndResultHidden();
        }

        public bool ShowGraph
        {
            set
            {
                SetSplitterPosition();
                _showGraph = value;
                ToggleGraph();
                ToggleResult();
            }
            get
            {
                return _showGraph;
            }
        }

        private void ToggleGraph()
        {
            if (!_showGraph)
            {
                GraphResultSplitter.Panel1Collapsed = true;
                GraphResultSplitter.IsSplitterFixed = true;
            }
            //hide
            if (_showGraph)
            {
                GraphResultSplitter.IsSplitterFixed = false;
                GraphResultSplitter.Panel1Collapsed = false;
            }
            CheckIfBothGraphAndResultHidden();
        }

        public bool ShowResult
        {
            set
            {
                SetSplitterPosition();
                _showResult = value;
                ToggleResult();
                ToggleGraph();
            }
            get
            {
                return _showResult;
            }
        }

        private void ToggleResult()
        {
            if (!_showResult)
            {
                GraphResultSplitter.Panel2Collapsed = true;
                GraphResultSplitter.IsSplitterFixed = true;
            }
            //hide
            if (_showResult)
            {
                GraphResultSplitter.IsSplitterFixed = false;
                GraphResultSplitter.Panel2Collapsed = false;
            }
            CheckIfBothGraphAndResultHidden();
        }

        private void SetSplitterPosition()
        {
            if (_showGraph && _showResult)
                _graphResultSplitterPos = GraphResultSplitter.SplitterDistance;

            if (_showEditor)
                _editorPositionFromBottom = GridEditorSplitter.Height - GridEditorSplitter.SplitterDistance;
        }

        private void CheckIfBothGraphAndResultHidden()
        {
            if (!_showGraph && !_showResult)
            {
                MainSplitter.Panel1Collapsed = true;
                MainSplitter.IsSplitterFixed = true;
                if (_showEditor)
                    GridEditorSplitter.SplitterDistance = GridEditorSplitter.Height - _editorPositionFromBottom;
            }
            else
            {   
                MainSplitter.Panel1Collapsed = false;
                MainSplitter.IsSplitterFixed = false;
                GridEditorSplitter.SplitterDistance = GridEditorSplitter.Height - _editorPositionFromBottom;
                GraphResultSplitter.SplitterDistance = _graphResultSplitterPos;
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
            MainSplitter = null;
            GraphResultSplitter = null;
            GridEditorSplitter = null;
        }
        #endregion
    }
}
