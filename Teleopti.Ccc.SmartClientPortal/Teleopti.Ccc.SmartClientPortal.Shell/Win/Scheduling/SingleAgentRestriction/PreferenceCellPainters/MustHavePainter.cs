using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class MustHavePainter: PreferenceCellPainterBase
    {        
        private readonly Image _mustHaveImage = Resources.heart_8x8;

        public MustHavePainter()
            : base(null)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintMustHave();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            int start = clientRectangle.Right - 10;
            
            Point point = new Point(start, clientRectangle.Y+2);
            Size size = new Size(9,9);
            Rectangle rect = new Rectangle(point, size);
            g.DrawImage(_mustHaveImage, MiddleX(rect) - (_mustHaveImage.Width / 2),
                        MiddleY(rect) - (_mustHaveImage.Height / 2));
        }
    }
}