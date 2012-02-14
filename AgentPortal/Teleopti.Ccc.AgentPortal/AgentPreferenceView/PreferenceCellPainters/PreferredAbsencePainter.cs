using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class PreferredAbsencePainter : PreferenceCellPainterBase
    {
        public PreferredAbsencePainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintPreferredAbsence();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            if(g == null)
                throw new ArgumentNullException("g");
            if (preference == null)
                throw new ArgumentNullException("preference");
            if (format == null)
                throw new ArgumentNullException("format");
            if(cellValue == null)
                throw new ArgumentNullException("cellValue");

            var rect = RestrictionRect(clientRectangle);

            if (cellValue.IsWorkday)
            {
                using (var lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, preference.Absence.Color, 90,false))
                {
                    GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
                }
            }
            else
            {
                using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, preference.Absence.Color, Color.LightGray))
                {
                    g.FillRectangle(brush, rect);
                }    
            }

            g.DrawString(LongOrShortText(preference.Absence.Name, preference.Absence.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
            format.Alignment = StringAlignment.Center;

            if (effectiveRestriction == null) return;
            var text = effectiveRestriction.WorkTimeLimitation.StartTimeString;
            rect = EffectiveRect(clientRectangle);
            g.DrawString(text, GridControlBase.Font, Brushes.Black, new RectangleF(rect.Left, rect.Top + 12, rect.Width, 12), format);
        }
    }
}
