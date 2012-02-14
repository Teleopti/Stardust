using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{
    /// <summary>
    /// Caches the temporary PersonPeriod list for a person and a period for peformance reasons.
    /// </summary>
    public class PersonPeriodDynamicCache : IPersonPeriodDynamicCache
    {
        private IPerson _person;
        private DateTimePeriod _period;
        private IList<IPersonPeriod> _cachedPersonPeriods;
        private bool _enabled;


        /// <summary>
        /// Gets the persons periods for the specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public IList<IPersonPeriod> PersonPeriods(IPerson person, DateTimePeriod period)
        {
            if (!_enabled || _person == null || !_person.Equals(person)
                || (_period != period))
            {
                _person = person;
                _period = period;
                DateOnlyPeriod dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
                _cachedPersonPeriods = person.PersonPeriods(dateOnlyPeriod);
            }
            return _cachedPersonPeriods;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PersonPeriodDynamicCache"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Deletes the cached data.
        /// </summary>
        public void DeleteCache()
        {
            _cachedPersonPeriods = null;
        }
    }
}
