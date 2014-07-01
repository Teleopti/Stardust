using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
    public class ResourceGridCutPasteHandler : BaseCutPasteHandler
    {
        private readonly WeakReference _owner;

        public ResourceGridCutPasteHandler(Control owner)
        {
            _owner = new WeakReference(owner);
        }

        public override void CopySpecial()
        {
            if (!_owner.IsAlive) return;

            var guiHelper = new ColorHelper();
            Control activeControl = guiHelper.GetActiveControl((Control) _owner.Target);
            var control = (GridControl)activeControl;
            GridHelper.CopySelectedValuesAndHeadersToPublicClipboard(control);
        }

        public override void Copy()
        {
            if (!_owner.IsAlive) return;
            
            var guiHelper = new ColorHelper();
            Control activeControl = guiHelper.GetActiveControl((Control) _owner.Target);
            var control = (GridControl)activeControl;
            control.CutPaste.Copy();
        }
    }
}