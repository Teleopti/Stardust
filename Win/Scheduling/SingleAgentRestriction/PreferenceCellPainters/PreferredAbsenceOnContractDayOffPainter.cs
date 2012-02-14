using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    public class PreferredAbsenceOnContractDayOffPainter : PreferenceCellPainterBase
    {
        public PreferredAbsenceOnContractDayOffPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            var helper = new PainterHelper(cellValue);
            return helper.CanPaintPreferredAbsenceOnContractDayOff();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            using (var lBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, cellValue.EffectiveRestriction.Absence.DisplayColor))
            {
                GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
            }

            var startSize = g.MeasureString(cellValue.ShiftLengthScheduledShift, GridControlBase.Font);
            var startSizeStartEnd = g.MeasureString(cellValue.StartEndScheduledShift, GridControlBase.Font);
            var paddingLength = Math.Max((rect.Width - startSize.Width) / 2, 0);
            var paddingStartEnd = Math.Max((rect.Width - startSizeStartEnd.Width) / 2, 0);
            g.DrawString(LongOrShortText(cellValue.EffectiveRestriction.Absence.Description.Name, cellValue.EffectiveRestriction.Absence.Description.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
            format.Alignment = StringAlignment.Center;

            g.DrawString(cellValue.StartEndScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingStartEnd, rect.Top + 24);
            g.DrawString(cellValue.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingLength, rect.Top + 36);

        }
    }
}