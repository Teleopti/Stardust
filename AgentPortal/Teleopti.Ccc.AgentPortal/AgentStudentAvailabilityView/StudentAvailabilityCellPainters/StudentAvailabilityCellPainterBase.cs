using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    public abstract class StudentAvailabilityCellPainterBase : IStudentAvailabilityCellPainter
    {
        private readonly GridControlBase _gridControlBase;

        protected StudentAvailabilityCellPainterBase(GridControlBase gridControlBase)
        {
            _gridControlBase = gridControlBase;
        }

        protected GridControlBase GridControlBase
        {
            get { return _gridControlBase; }
        }

        protected static float MiddleX(Rectangle clientRectangle)
        {
            return clientRectangle.Left + (clientRectangle.Width / 2);
        }

        protected static float MiddleY(Rectangle clientRectangle)
        {
            return clientRectangle.Top + (clientRectangle.Height / 2);
        }

        protected static Rectangle RestrictionRect(Rectangle clientRectangle)
        {
            Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y + 15, clientRectangle.Width, clientRectangle.Height / 6);
            rect.Inflate(-6, 0);
            return rect;
        }

        protected static Rectangle EffectiveRect(Rectangle clientRectangle)
        {
            Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y + (clientRectangle.Height *5 / 9), clientRectangle.Width, clientRectangle.Height / 2);
            rect.Inflate(-6, 1);
            return rect;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static string LimitationToString(TimeLimitation limitation)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        protected string LongOrShortText(string name, string shortName, int maxWidth, Graphics g)
        {
            string text = name;
            SizeF size = g.MeasureString(text, _gridControlBase.Font);
            if (size.Width > maxWidth)
                text = shortName;
            return text;
        }

        protected static Rectangle EnlargeRectangle(Rectangle oldRect, double factor, bool vertically)
        {
            return vertically ? new Rectangle(oldRect.X, oldRect.Y, oldRect.Width,  (int) (oldRect.Height * factor)) : new Rectangle(oldRect.X, oldRect.Y, (int) (oldRect.Width * factor), oldRect.Height);
        }

        public abstract bool CanPaint(StudentAvailabilityCellData cellValue);

        public abstract void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format);
    }
}
