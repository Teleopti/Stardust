using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    public class ActivityPreferencePainter : PreferenceCellPainterBase
    {
        private readonly Image _magnifierImage = Resources.magnifier_8x8;
        public ActivityPreferencePainter() : base(null) { }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintActivityPreference();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            int start = clientRectangle.Right - 20;

            Point point = new Point(start, clientRectangle.Y + 2);
            Size size = new Size(9, 9);
            Rectangle rect = new Rectangle(point, size);
            g.DrawImage(_magnifierImage, MiddleX(rect) - (_magnifierImage.Width / (float)2),
                        MiddleY(rect) - (_magnifierImage.Height / (float)2));
        }
    }
}