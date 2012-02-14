using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 10/27/2010
    /// </remarks>
    public class DateDayModel
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/27/2010
        /// </remarks>
        public DateOnly Date { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateDayModel"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/27/2010
        /// </remarks>
        public DateDayModel(DateOnly date)
        {
            Date = date;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/27/2010
        /// </remarks>
        public override string ToString()
        {
            return Date.Day.ToString(CultureInfo.CurrentCulture);
        }
    }
}
