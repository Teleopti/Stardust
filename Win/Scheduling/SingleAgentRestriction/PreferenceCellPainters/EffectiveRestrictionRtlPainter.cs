﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class EffectiveRestrictionRtlPainter : PreferenceCellPainterBase
    {
        private readonly Image _startImage = Resources.StartTime;
        private readonly Image _endImage = Resources.EndTime;
        private readonly Image _workImage = Resources.WorkTime;
        
        public EffectiveRestrictionRtlPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            bool rightToLeft = false;
            if (GridControlBase.RightToLeft == RightToLeft.Yes)
                rightToLeft = true;
            return helper.CanPaintEffectiveRestrictionRightToLeft(rightToLeft);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = EffectiveRect(clientRectangle);
            var startLimitationAsString = LimitationToString(effectiveRestriction.StartTimeLimitation);
            var endLimitationAsString = LimitationToString(effectiveRestriction.EndTimeLimitation);
            var workLimitationAsString = WorkTimeLimitationToString(effectiveRestriction.WorkTimeLimitation);

            var startSize = g.MeasureString(startLimitationAsString, GridControlBase.Font);
            var endSize = g.MeasureString(endLimitationAsString, GridControlBase.Font);
            var workSize = g.MeasureString(workLimitationAsString, GridControlBase.Font);
            var largestWidth = (int)Math.Max(startSize.Width,Math.Max(endSize.Width,workSize.Width));
            largestWidth = largestWidth + 1 + _startImage.Width;
            var padding = Math.Max((rect.Width - largestWidth)/2, 0);

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

        private static string LimitationToString(ILimitation limitation)
        {
            if (!limitation.StartTime.HasValue && !limitation.EndTime.HasValue)
                return string.Empty;

            string start = UserTexts.Resources.NA;
            if (limitation.StartTime.HasValue)
                start = limitation.StartTimeString;

            string end = UserTexts.Resources.NA;
            if (limitation.EndTime.HasValue)
                end = limitation.EndTimeString;

            return string.Concat(start, " - ", end);
        }
        private static string WorkTimeLimitationToString(ILimitation limitation)
        {
            CultureInfo culture =
                TeleoptiPrincipal.Current.Regional.Culture;
            if (!limitation.StartTime.HasValue && !limitation.EndTime.HasValue)
                return string.Empty;
            if (!limitation.StartTime.HasValue && !limitation.EndTime.HasValue)
                return string.Empty;

            string start = UserTexts.Resources.NA;
            if (limitation.StartTime.HasValue)
                start = TimeHelper.GetLongHourMinuteTimeString(limitation.StartTime.Value, culture);

            string end = UserTexts.Resources.NA;
            if (limitation.EndTime.HasValue)
                end = TimeHelper.GetLongHourMinuteTimeString(limitation.EndTime.Value, culture);

            return string.Concat(start, " - ", end);
        }
    }
}