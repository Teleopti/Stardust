using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction
{
    public partial class TeleoptiLessIntelligentSplitContainer : SplitContainerAdv
    {
        public TeleoptiLessIntelligentSplitContainer()
        {
            InitializeComponent();
        }

        public SizeF AutoScaleDimensions { get; set; }
        public AutoScaleMode AutoScaleMode { get; set; }

        protected override void OnClick(EventArgs e)
        {
            //Do not call base method
            //base.OnClick(e);
        }

    }
}