using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class NotValidPainter : PreferenceCellPainterBase
    {
        private readonly Image _notValidatedImage = Resources.ccc_Cancel_16x16;
        private static readonly NotValidSpecification NotValidSpecification = new NotValidSpecification();
        
        public NotValidPainter() : base(null)
        {}

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            return NotValidSpecification.IsSatisfiedBy(cellValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            g.DrawImage(_notValidatedImage, clientRectangle.Left + clientRectangle.Width - _notValidatedImage.Width - 10, clientRectangle.Top + 3);
        }

    }
}