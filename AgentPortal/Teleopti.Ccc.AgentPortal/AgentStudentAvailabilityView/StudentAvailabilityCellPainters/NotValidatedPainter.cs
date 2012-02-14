using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    class NotValidatedPainter : StudentAvailabilityCellPainterBase
    {
        private readonly Image _notValidatedImage = Resources.ccc_ForecastValidate;
        private static readonly NotValidatedSpecification NotValidatedSpecification = new NotValidatedSpecification();
        
        public NotValidatedPainter() : base(null)
        {}

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            return NotValidatedSpecification.IsSatisfiedBy(cellValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = EffectiveRect(clientRectangle);
            g.DrawImage(_notValidatedImage, MiddleX(rect) - (_notValidatedImage.Width/2),
                MiddleY(rect) - (_notValidatedImage.Height / 2));
        }
    }
}
