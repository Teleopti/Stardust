using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    public class ViolatesNightlyRestPainter : PreferenceCellPainterBase
    {
        private readonly Image _invalidEffectiveImage = Resources.ccc_Cancel_32x32;

        public ViolatesNightlyRestPainter(GridControlBase gridControlBase): base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintViolatesNightlyRest();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            if(g == null)
                throw new ArgumentNullException("g");

            var rect = EffectiveRect(clientRectangle);
            g.DrawImage(_invalidEffectiveImage, MiddleX(rect) - (_invalidEffectiveImage.Width / 2), MiddleY(rect) - (_invalidEffectiveImage.Height / 2));
        }
    }
}
