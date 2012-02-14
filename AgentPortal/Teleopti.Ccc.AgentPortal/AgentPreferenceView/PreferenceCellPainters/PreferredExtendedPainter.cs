﻿using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class PreferredExtendedPainter : PreferenceCellPainterBase
    {
        public PreferredExtendedPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintPreferredExtended();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            Rectangle gradRect = new Rectangle(rect.Location, rect.Size);
            gradRect.Inflate(0, 16);
            gradRect.Offset(0, 15);
            using (LinearGradientBrush lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, Color.LightGray, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, rect, 7, lBrush, 0);
            }
            var templateText = UserTexts.Resources.Extended;
            if (preference.TemplateName != null)
            {
                templateText = preference.TemplateName; 
            }
            g.DrawString(LongOrShortText(templateText, UserTexts.Resources.ExtDot, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
        }
    }
}
