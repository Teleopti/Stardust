using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Class responsable for some apperence of the grid in detail view
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2007-11-08
    /// </remarks>
    public static class DetailViewHelper
    {
        /// <summary>
        /// The height of the absence rectangle
        /// </summary>
        public const int AbsenceHeight = 6;

        /// <summary>
        /// Gets the abs and dayOff rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="displayMode">The display mode.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        public static Rectangle GetAbsRect(Rectangle rect, DisplayMode displayMode)
        {
            Rectangle absRect;
            switch (displayMode)
            {
                case DisplayMode.EndsToday:
                    absRect =
                        new Rectangle(rect.X, 0, rect.Width - rect.Width / 2, AbsenceHeight);
                    break;
                case DisplayMode.BeginsToday:
                    absRect =
                        new Rectangle(rect.X + rect.Width / 2, 0, rect.Width / 2, AbsenceHeight);
                    break;
                case DisplayMode.BeginsAndEndsToday:
                    absRect =
                        new Rectangle(rect.X + rect.Width / 4, 0, rect.Width - rect.Width / 2, AbsenceHeight);
                    break;
                case DisplayMode.WholeDay:
                case DisplayMode.DayOff:
                    absRect = new Rectangle(rect.X, 0, rect.Width, AbsenceHeight);
                    break;
                default:
                    throw new InvalidEnumArgumentException("displayMode", (int)displayMode, typeof(DisplayMode));
            }
            return absRect;
        }

        /// <summary>
        /// Gets the ass rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="displayMode">The display mode.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        public static Rectangle GetAssRect(Rectangle rect, DisplayMode displayMode)
        {
            switch (displayMode)
            {
                case DisplayMode.BeginsToday:
                    rect = new Rectangle(rect.X + rect.Width / 2, rect.Y + 2, rect.Width, rect.Height);
                    break;
                case DisplayMode.EndsToday:
                    rect = new Rectangle(rect.X - rect.Width / 2, rect.Y + 2, rect.Width, rect.Height);
                    break;
                case DisplayMode.WholeDay:
                case DisplayMode.BeginsAndEndsToday:
                    rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                    break;
                 
                default:
                    throw new InvalidEnumArgumentException("displayMode", (int)displayMode, typeof(DisplayMode));
            }
            return rect;
        }
    }
}
