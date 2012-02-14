using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class PreferredDayOffPainter : PreferenceCellPainterBase
    {
        public PreferredDayOffPainter(GridControlBase gridControlBase):base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintPreferredDayOff();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, effectiveRestriction.DayOffTemplate.DisplayColor, Color.LightGray))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawString(LongOrShortText(effectiveRestriction.DayOffTemplate.Description.Name, effectiveRestriction.DayOffTemplate.Description.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
        }
    }
}