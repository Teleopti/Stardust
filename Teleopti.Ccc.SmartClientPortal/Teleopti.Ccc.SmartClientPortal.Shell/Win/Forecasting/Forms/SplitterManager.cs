using System;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Tools.Enums;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    class SplitterManager : IDisposable
    {
        private bool _showGraph = true;
        private bool _showSkillView = true;
        public SplitContainerAdv MainSplitter { get; set; }
        public SplitContainerAdv WorkSkillSplitter { get; set; }
        public bool ShowGraph
        {
            set
            {
               _showGraph = value;
                ToggleGraph();
            }
            get
            {
                return _showGraph;
            }
        }

        public bool ShowSkillView
        {
            set
            {
                _showSkillView = value;
                ToggleSkill();
            }
            get
            {
                return _showSkillView;
            }
        }

        private void ToggleSkill()
        {
            if (_showSkillView)
            {
                WorkSkillSplitter.Panel2Collapsed = false;
                WorkSkillSplitter.IsSplitterFixed = false;
                WorkSkillSplitter.Style = Style.Default;
            }
            else
            {
                WorkSkillSplitter.Panel2Collapsed = true;
                WorkSkillSplitter.IsSplitterFixed = true;
                WorkSkillSplitter.Style = Style.None;
            }
        }

        private void ToggleGraph()
        {
            if (_showGraph)
            {
                MainSplitter.Panel1Collapsed = false;
                MainSplitter.IsSplitterFixed = false;
                MainSplitter.Style = Style.Default;
            }
            else
            {
                MainSplitter.Panel1Collapsed = true;
                MainSplitter.IsSplitterFixed = true;
                MainSplitter.Style = Style.None;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void ReleaseManagedResources()
        {
            MainSplitter = null;
            WorkSkillSplitter = null;
        }
    }
}
