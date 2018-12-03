using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels
{
    public class LengthToDateCalculator
    {
        private readonly double _length;
        private DateOnlyPeriod _period;
        
        private static int DiffInDays(DateOnly start,DateOnly end)
        {
            return (int)end.Subtract(start).TotalDays;
        }

        public LengthToDateCalculator(DateOnlyPeriod period, double length)
        {
            _period = period;
            _length = length;
        }

        /// <summary>
        /// Returns the position represented by the DateTime
        /// </summary>
        /// <param name="dateTime">The date time.(in Utc)</param>
        /// <returns>
        /// Returns the position represented by the DateTime
        /// </returns>
        public double PositionFromDateTime(DateOnly dateTime)
        {
            return PositionFromDateTime(dateTime, false);
        }

        public Rectangle RectangleFromDateTimePeriod(DateOnlyPeriod datePeriod, Point location, int height, bool rightToLeft)
        {
            int xStartPosition = (int)Math.Round(PositionFromDateTime(datePeriod.StartDate, rightToLeft) + location.X, 0);
            int xEndPosition = (int)Math.Round(PositionFromDateTime(datePeriod.EndDate, rightToLeft) + location.X, 0);
            int width = Math.Abs(xEndPosition - xStartPosition);

            return new Rectangle(rightToLeft ? xEndPosition : xStartPosition, location.Y, width, height);
        }

        public double PositionFromDateTime(DateOnly date, bool rightToLeft)
        {
            double positionNormal = (_length * DiffInDays(_period.StartDate, date)) / DiffInDays(_period.StartDate, _period.EndDate);
            return rightToLeft ? _length - positionNormal : positionNormal;
        }
    }
}
