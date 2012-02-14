using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class ScheduledPainter : PreferenceCellPainterBase
    {
        public ScheduledPainter()
            : base(null)
        {}

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            return (cellValue.SchedulingOption.UseScheduling && (cellValue.HasDayOff || cellValue.HasShift || cellValue.HasFullDayAbsence));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.White)))
            {
                g.FillRectangle(brush, clientRectangle);
            }
        }
    }
}
