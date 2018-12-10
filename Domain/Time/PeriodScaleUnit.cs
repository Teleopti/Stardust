using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Scale Unit Type for time measurement
    /// </summary>
    public enum PeriodScaleUnitType
    {
        /// <summary>
        /// Days
        /// </summary>
        Days,
        /// <summary>
        /// Weeks
        /// </summary>
        Weeks,
        /// <summary>
        /// Months
        /// </summary>
        Months,
        /// <summary>
        /// Years
        /// </summary>
        Years,
        /// <summary>
        /// Decades
        /// </summary>
        Decades,
        /// <summary>
        /// Centuries
        /// </summary>
        Centuries,
        /// <summary>
        /// Millenniums
        /// </summary>
        Millenniums,
        /// <summary>
        /// Eras
        /// </summary>
        Eras,
        /// <summary>
        /// Eons
        /// </summary>
        Eons
    }

    
    /// <summary>
    /// Unit used for defining timescale.
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2008-01-31
    /// </remarks>
    public class PeriodScaleUnit
    {
        private readonly PeriodScaleUnitType _periodScaleUnitType;
        private readonly string _displayName;

        private PeriodScaleUnit() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScaleUnit"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="displayName">The display name.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        public PeriodScaleUnit(PeriodScaleUnitType type, string displayName):this()
        {
            _periodScaleUnitType = type;
            _displayName = displayName;
        }

        /// <summary>
        /// Gets or sets the type of the unit.
        /// </summary>
        /// <value>The type of the unit.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        public PeriodScaleUnitType UnitType
        {
            get { return _periodScaleUnitType; }
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        public string DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>
        /// Gets the scale unit date time period.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="numberOf">Number of</param>
        /// <param name="startingDateTime">The starting date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-01-31
        /// </remarks>
        public static DateOnlyPeriod GetScaleUnitDateTimePeriod(PeriodScaleUnitType type, int numberOf, DateOnly startingDateTime)
        {
            DateOnlyPeriod dateOnlyPeriod;
            DateOnly dateTime = AddWithPeriodScaleUnit(startingDateTime, type, numberOf);
            if (numberOf < 0)
            {
                dateOnlyPeriod = new DateOnlyPeriod(dateTime, startingDateTime.AddDays(-1));
            }
            else
            {
                dateOnlyPeriod = new DateOnlyPeriod(startingDateTime, dateTime);
            }
            return dateOnlyPeriod;
        }

        /// <summary>
        /// Adds the with a time according to type.
        /// </summary>
        /// <param name="startingDateTime">The starting date time.</param>
        /// <param name="type">The type.</param>
        /// <param name="numberOf">The number of.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        private static DateOnly AddWithPeriodScaleUnit(DateOnly startingDateTime, PeriodScaleUnitType type, int numberOf)
        {
            DateOnly dateTime = startingDateTime;
            switch(type)
            {
                case PeriodScaleUnitType.Days:
                    dateTime = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddDays(startingDateTime.Date, numberOf));
                    break;
                case PeriodScaleUnitType.Weeks:
                    dateTime = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddWeeks(startingDateTime.Date, numberOf));
                    break;
                case PeriodScaleUnitType.Months:
                    dateTime = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddMonths(startingDateTime.Date, numberOf));
                    break;
                case PeriodScaleUnitType.Years:
                    dateTime = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddYears(startingDateTime.Date, numberOf));
                    break;
            }
            return dateTime;
        }
    }
 }