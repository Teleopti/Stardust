using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    class EffectiveRestrictionRtlPainter : StudentAvailabilityCellPainterBase
    {
        private readonly Image _startImage = Resources.StartTime;
        private readonly Image _endImage = Resources.EndTime;
        private readonly Image _workImage = Resources.WorkTime;
        private readonly Image _invalidEffectiveImage = Resources.ccc_Cancel_32x32;

        public EffectiveRestrictionRtlPainter(GridControlBase gridControlBase)
            : base(gridControlBase)
        {
        }

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintEffectiveRestrictionRightToLeft(GridControlBase.IsRightToLeft());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = EffectiveRect(clientRectangle);
            if (effectiveRestriction.Invalid)
            {
                g.DrawImage(_invalidEffectiveImage, MiddleX(rect) - (_invalidEffectiveImage.Width / 2),
                            MiddleY(rect) - (_invalidEffectiveImage.Height / 2));
                return;
            }

            var startLimitationAsString = LimitationToString(effectiveRestriction.StartTimeLimitation);
            var endLimitationAsString = LimitationToString(effectiveRestriction.EndTimeLimitation);
            var workLimitationAsString = LimitationToString(effectiveRestriction.WorkTimeLimitation);

            var startSize = g.MeasureString(startLimitationAsString, GridControlBase.Font);
            var endSize = g.MeasureString(endLimitationAsString, GridControlBase.Font);
            var workSize = g.MeasureString(workLimitationAsString, GridControlBase.Font);
            var largestWidth = (int)Math.Max(startSize.Width, Math.Max(endSize.Width, workSize.Width));
            largestWidth = largestWidth + 1 + _startImage.Width;
            var padding = Math.Max((rect.Width - largestWidth) / 2, 0);

            if (!string.IsNullOrEmpty(startLimitationAsString))
                g.DrawImage(_endImage, rect.Right - _endImage.Width, rect.Top + 2);
            g.DrawString(startLimitationAsString, GridControlBase.Font, Brushes.Black,
                         rect.Right - padding - _endImage.Width - startSize.Width, rect.Top + 0);

            if (!string.IsNullOrEmpty(endLimitationAsString))
                g.DrawImage(_startImage, rect.Right - padding - _startImage.Width, rect.Top + 14);
            g.DrawString(endLimitationAsString, GridControlBase.Font, Brushes.Black,
                         rect.Right - padding - _startImage.Width - endSize.Width, rect.Top + 12);

            if (!string.IsNullOrEmpty(workLimitationAsString))
                g.DrawImage(_workImage, rect.Right - padding - _workImage.Width, rect.Top + 26);
            g.DrawString(workLimitationAsString, GridControlBase.Font, Brushes.Black,
                         rect.Right - padding - _workImage.Width - workSize.Width, rect.Top + 24);
        }
    }
}
