using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class NotValidatedPainter : PreferenceCellPainterBase
    {
        private readonly Image _notValidatedImage = Resources.ccc_ForecastValidate;
        private static readonly NotValidatedSpecification NotValidatedSpecification = new NotValidatedSpecification();
        
        public NotValidatedPainter() : base(null)
        {}

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            return NotValidatedSpecification.IsSatisfiedBy(cellValue);
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = EffectiveRect(clientRectangle);
            g.DrawImage(_notValidatedImage, MiddleX(rect) - (_notValidatedImage.Width/2),
                MiddleY(rect) - (_notValidatedImage.Height / 2));
        }
    }
}
