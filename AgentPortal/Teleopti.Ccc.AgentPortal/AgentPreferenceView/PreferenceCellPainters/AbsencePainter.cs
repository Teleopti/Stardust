using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class AbsencePainter : PreferenceCellPainterBase
    {
        public AbsencePainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintAbsence();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            using (LinearGradientBrush lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, cellValue.DisplayColor, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
            }
            g.DrawString(LongOrShortText(cellValue.DisplayName, cellValue.DisplayShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
            format.Alignment = StringAlignment.Center;

            string text = effectiveRestriction.WorkTimeLimitation.StartTimeString;
            rect = EffectiveRect(clientRectangle);
            g.DrawString(text, GridControlBase.Font, Brushes.Black, new RectangleF(rect.Left, rect.Top + 12, rect.Width, 12), format);
        }
    }
}
