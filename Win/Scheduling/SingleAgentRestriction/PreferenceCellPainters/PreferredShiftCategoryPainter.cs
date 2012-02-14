using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class PreferredShiftCategoryPainter : PreferenceCellPainterBase
    {
        public PreferredShiftCategoryPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintPreferredShiftCategory();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            Rectangle gradRect = new Rectangle(rect.Location, rect.Size);
            gradRect.Inflate(0, 16);
            gradRect.Offset(0, 15);
            using (LinearGradientBrush lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, effectiveRestriction.ShiftCategory.DisplayColor, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, rect, 7, lBrush, 0);
            }
            g.DrawString(LongOrShortText(effectiveRestriction.ShiftCategory.Description.Name, effectiveRestriction.ShiftCategory.Description.ShortName, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
        }
    }
}