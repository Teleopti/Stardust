using System.Drawing;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
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
            g.DrawImage(_notValidatedImage, clientRectangle.Left + clientRectangle.Width - _notValidatedImage.Width - 3, clientRectangle.Top + 3);
        }

    }
}