using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class PreferredDayOffPainter : PreferenceCellPainterBase
    {
        public PreferredDayOffPainter(GridControlBase gridControlBase):base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintPreferredDayOff(); 
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, preference.DayOff.DisplayColor, Color.LightGray))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawString(LongOrShortText(preference.DayOff.Name, preference.DayOff.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
        }
    }
}
