using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-10-29
    /// </remarks>
    public class LengthToTimeCalculator : ILengthToTimeCalculator
    {
        private readonly double _length;
        private DateTimePeriod _period;
        
        private static long DiffInTicks(DateTime start,DateTime end)
        {
            return end.Subtract(start).Ticks;
        }

        public LengthToTimeCalculator(DateTimePeriod period,double length)
        {
            _period = period;
            _length = length;
        }

        /// <summary>
        /// Returns the DateTime represented by the position
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        /// Returns the DateTime represented by the position
        /// </returns>
        public DateTime DateTimeFromPosition(double position)
        {
            return DateTimeFromPosition(position, false);
        }


        /// <summary>
        /// Returns the position represented by the DateTime
        /// </summary>
        /// <param name="dateTime">The date time.(in Utc)</param>
        /// <returns>
        /// Returns the position represented by the DateTime
        /// </returns>
        public double PositionFromDateTime(DateTime dateTime)
        {
            return PositionFromDateTime(dateTime, false);
        }

        public Rectangle RectangleFromDateTimePeriod(DateTimePeriod dateTimePeriod, Point location, int height, bool rightToLeft)
        {
            int xStartPosition = (int)Math.Round(PositionFromDateTime(dateTimePeriod.StartDateTime,rightToLeft) + location.X,0);
            int xEndPosition = (int)Math.Round(PositionFromDateTime(dateTimePeriod.EndDateTime,rightToLeft) + location.X,0);
            int width = Math.Abs(xEndPosition - xStartPosition);

            return new Rectangle(rightToLeft ? xEndPosition : xStartPosition, location.Y, width, height);
        }

        public double PositionFromDateTime(DateTime dateTime, bool rightToLeft)
        {
            InParameter.VerifyDateIsUtc(nameof(dateTime), dateTime);
            double positionNormal = (_length * DiffInTicks(_period.StartDateTime, dateTime)) / DiffInTicks(_period.StartDateTime, _period.EndDateTime);
            return rightToLeft ? _length - positionNormal : positionNormal;
        }

        public DateTime DateTimeFromPosition(double position, bool rightToLeft)
        {
            TimeSpan diff = TimeSpan.FromTicks((long)(DiffInTicks(_period.StartDateTime, _period.EndDateTime) * (position / _length)));
            return rightToLeft ? _period.EndDateTime.Add(diff.Negate()) : _period.StartDateTime.Add(diff);
        }
    }
}
