using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class EffectiveRestrictionPainter : PreferenceCellPainterBase
    {
        private readonly Image _startImage = Resources.StartTime;
        private readonly Image _endImage = Resources.EndTime;
        private readonly Image _workImage = Resources.WorkTime;
        private readonly Image _invalidEffectiveImage = Resources.ccc_Cancel_32x32;
        
        public EffectiveRestrictionPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintEffectiveRestriction(GridControlBase.IsRightToLeft());
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = EffectiveRect(clientRectangle);
            if (effectiveRestriction.Invalid)
            {
                g.DrawImage(_invalidEffectiveImage, MiddleX(rect) - (_invalidEffectiveImage.Width / 2),
                            MiddleY(rect) - (_invalidEffectiveImage.Height / 2));
                return;
            }

            if (cellValue.HasShift)
            {
                format.Alignment = StringAlignment.Center;
                string text = string.Concat(effectiveRestriction.StartTimeLimitation.StartTimeString, " - ", effectiveRestriction.EndTimeLimitation.EndTimeString);
                g.DrawString(text, GridControlBase.Font, Brushes.Black, new RectangleF(rect.Left, rect.Top + 0, rect.Width, 12), format);
                text = effectiveRestriction.WorkTimeLimitation.StartTimeString;
                g.DrawString(text, GridControlBase.Font, Brushes.Black, new RectangleF(rect.Left, rect.Top + 12, rect.Width, 12), format);
            }
            else
            {
                var startLimitationAsString = LimitationToString(effectiveRestriction.StartTimeLimitation);
                var endLimitationAsString = LimitationToString(effectiveRestriction.EndTimeLimitation);
                var workLimitationAsString = LimitationToString(effectiveRestriction.WorkTimeLimitation);

                var startSize = g.MeasureString(startLimitationAsString, GridControlBase.Font);
                var endSize = g.MeasureString(endLimitationAsString, GridControlBase.Font);
                var workSize = g.MeasureString(workLimitationAsString, GridControlBase.Font);
                var largestWidth = (int)Math.Max(startSize.Width,Math.Max(endSize.Width,workSize.Width));
                largestWidth = largestWidth + 1 + _startImage.Width;
                var padding = Math.Max((rect.Width - largestWidth)/2, 0);

                if (!string.IsNullOrEmpty(startLimitationAsString))
                    g.DrawImage(_startImage, rect.Left + padding, rect.Top + 2);
                g.DrawString(startLimitationAsString, GridControlBase.Font, Brushes.Black,
                             rect.Left + padding + 1 + _startImage.Width, rect.Top + 0);

                if (!string.IsNullOrEmpty(endLimitationAsString))
                    g.DrawImage(_endImage, rect.Left + padding, rect.Top + 14);
                g.DrawString(endLimitationAsString, GridControlBase.Font, Brushes.Black,
                             rect.Left + padding + 1 + _startImage.Width, rect.Top + 12);

                if (!string.IsNullOrEmpty(workLimitationAsString))
                    g.DrawImage(_workImage, rect.Left + padding, rect.Top + 26);
                g.DrawString(workLimitationAsString, GridControlBase.Font, Brushes.Black,
                             rect.Left + padding + 1 + _startImage.Width, rect.Top + 24);
            }
        }

        private static string LimitationToString(TimeLimitation limitation)
        {
            if (!limitation.MinTime.HasValue && !limitation.MaxTime.HasValue)
                return string.Empty;

            string start = UserTexts.Resources.NA;
            if (limitation.MinTime.HasValue)
                start = limitation.StartTimeString;

            string end = UserTexts.Resources.NA;
            if (limitation.MaxTime.HasValue)
                end = limitation.EndTimeString;

            return string.Concat(start, " - ", end);
        }
    }
}
