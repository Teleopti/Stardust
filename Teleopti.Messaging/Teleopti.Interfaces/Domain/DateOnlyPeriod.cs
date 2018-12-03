using System;

namespace Teleopti.Interfaces.Domain
{
    [Serializable]
    public struct DateOnlyPeriod : IEquatable<DateOnlyPeriod>
    {
        private MinMax<DateOnly> period;
        private const string DATETIME_SEPARATOR = " - ";

		public DateOnlyPeriod(DateTime startDate, DateTime endDate) : this(new DateOnly(startDate), new DateOnly(endDate))
		{
		}
        
        public DateOnlyPeriod(DateOnly startDate, DateOnly endDate)
        {
            period = new MinMax<DateOnly>(startDate, endDate);
        }
		
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyPeriod"/> struct.
        /// </summary>
        /// <param name="startYear">The start year.</param>
        /// <param name="startMonth">The start month.</param>
        /// <param name="startDay">The start day.</param>
        /// <param name="endYear">The end year.</param>
        /// <param name="endMonth">The end month.</param>
        /// <param name="endDay">The end day.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-04-23
        /// </remarks>
        public DateOnlyPeriod(int startYear, 
                                int startMonth, 
                                int startDay, 
                                int endYear, 
                                int endMonth, 
                                int endDay)
        {
            period = new MinMax<DateOnly>(new DateOnly(startYear, startMonth, startDay), 
                                            new DateOnly(endYear, endMonth, endDay));
        }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateOnly StartDate => period.Minimum;

		/// <summary>
        /// Gets the end date.
        /// </summary>
        /// <value>The end date.</value>
        public DateOnly EndDate => period.Maximum;
		
	    /// <summary>
        /// Gets the date string.
        /// </summary>
        /// <value>The date string.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public string DateString => StartDate.Date.ToShortDateString() + DATETIME_SEPARATOR + EndDate.Date.ToShortDateString();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToShortDateString(IFormatProvider formatProvider)
		{
			
				return
					StartDate.ToShortDateString(formatProvider) + DATETIME_SEPARATOR + EndDate.ToShortDateString(formatProvider);
			
		}
        
        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return DateString;
        }

        /// <summary>
        /// Converts into a DateTimePeriod containing the whole last day of this period.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public DateTimePeriod ToDateTimePeriod(TimeZoneInfo info)
        {

            return new DateTimePeriod(info.SafeConvertTimeToUtc(period.Minimum.Date),
                                      info.SafeConvertTimeToUtc(period.Maximum.AddDays(1).Date));
        }
		
        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public bool Equals(DateOnlyPeriod other)
        {
            return other.StartDate == StartDate &&
                   other.EndDate == EndDate;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public static bool operator ==(DateOnlyPeriod per1, DateOnlyPeriod per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(DateOnlyPeriod per1, DateOnlyPeriod per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public override bool Equals(object obj)
		{
			return obj is DateOnlyPeriod onlyPeriod && Equals(onlyPeriod);
		}

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-17    
        /// </remarks>
        public override int GetHashCode()
        {
            return period.GetHashCode();
        }
    }
}
