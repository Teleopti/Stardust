using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class EffectiveRestrictionPainter : PreferenceCellPainterBase
    {
        private readonly Image _startImage = Resources.StartTime;
        private readonly Image _endImage = Resources.EndTime;
        private readonly Image _workImage = Resources.WorkTime;
        
        public EffectiveRestrictionPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            bool rightToLeft = false;
            if (GridControlBase.RightToLeft ==  RightToLeft.Yes)
                rightToLeft = true;
            return helper.CanPaintEffectiveRestriction(rightToLeft);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            if (cellValue.EffectiveRestriction != null && cellValue.EffectiveRestriction.Absence != null)
                return;

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
                TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
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