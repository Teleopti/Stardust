using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress
{
    [System.ComponentModel.DesignerCategory("code")]
    [System.Windows.Forms.Design.ToolStripItemDesignerAvailability(System.Windows.Forms.Design.ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripSpinningProgressControl : ToolStripControlHost
    {
        public ToolStripSpinningProgressControl()
            : base(createControlInstance(), "ToolStripSpinningProgress")
        {
        }

        public SpinningProgressControl SpinningProgressControl
        {
            get { return (SpinningProgressControl)Control; }
        }

// ReSharper disable RedundantOverridenMember
        public override Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }
// ReSharper restore RedundantOverridenMember

        public new bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        public Color InactiveSegmentColor
        {
            get
            {
                return SpinningProgressControl.InactiveSegmentColor;
            }
            set
            {
                SpinningProgressControl.InactiveSegmentColor = value;
            }
        }

        public Color ActiveSegmentColor
        {
            get
            {
                return SpinningProgressControl.ActiveSegmentColor;
            }
            set
            {
                SpinningProgressControl.ActiveSegmentColor = value;
            }
        }
 
        public Color TransitionSegmentColor
        {
            get
            {
                return SpinningProgressControl.TransitionSegmentColor;
            }
            set
            {
                SpinningProgressControl.TransitionSegmentColor = value;
            }
        }

        public bool BehindTransitionSegmentIsActive
        {
            get
            {
                return SpinningProgressControl.BehindTransitionSegmentIsActive;
            }
            set
            {
                SpinningProgressControl.BehindTransitionSegmentIsActive = value;
            }
        }

        public int TransitionSegment
        {
            get
            {
                return SpinningProgressControl.TransitionSegment;
            }
            set
            {
                if (value > 11 || value < -1)
                {
                    throw new ArgumentException("TransistionSegment must be between -1 and 11");
                }
                SpinningProgressControl.TransitionSegment = value;
            }
        }

        private static Control createControlInstance()
        {
            SpinningProgressControl c = new SpinningProgressControl();
            c.AutoSize = false;
            c.Height = 16;

            return c;
        }
    }
}
