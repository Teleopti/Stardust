using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Team.
    /// </summary>
    public class PersonBelongsToTeamSpecification : Specification<IPerson>
    {
        private readonly IList<ITeam> _teamCollection;
        private readonly DateTimePeriod _dateTimePeriod;
        private DateOnlyPeriod? _cachedDateOnlyPeriod;
        private string _cachedTimeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToTeamSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="team">The team.</param>
        public PersonBelongsToTeamSpecification(DateTimePeriod dateTimePeriod, ITeam team)
        {
            _teamCollection = new List<ITeam> { team };
            _dateTimePeriod = dateTimePeriod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToTeamSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="teamCollection">The team collection.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-13
        /// </remarks>
        public PersonBelongsToTeamSpecification(DateTimePeriod dateTimePeriod, IList<ITeam> teamCollection)
        {
            _teamCollection = teamCollection;
            _dateTimePeriod = dateTimePeriod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToTeamSpecification"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="teamCollection">The team collection.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-13
        /// </remarks>
        public PersonBelongsToTeamSpecification(DateTime dateTime, IList<ITeam> teamCollection) 
            : this(new DateTimePeriod(TimeZoneInfo.ConvertTimeToUtc(dateTime),
                   TimeZoneInfo.ConvertTimeToUtc(dateTime)), teamCollection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToTeamSpecification"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="team">The team.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-13
        /// </remarks>
        public PersonBelongsToTeamSpecification(DateTime dateTime, ITeam team)
            : this(new DateTimePeriod(TimeZoneInfo.ConvertTimeToUtc(dateTime), TimeZoneInfo.ConvertTimeToUtc(dateTime)), team)
        {
        }

        public PersonBelongsToTeamSpecification(DateOnlyPeriod period, ITeam team)
            : this(period.ToDateTimePeriod(TimeZoneHelper.CurrentSessionTimeZone), team)
        {
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            ICccTimeZoneInfo timeZoneInfo = obj.PermissionInformation.DefaultTimeZone();
            if (!_cachedDateOnlyPeriod.HasValue || _cachedTimeZone!=timeZoneInfo.Id)
            {
                _cachedDateOnlyPeriod = _dateTimePeriod.ToDateOnlyPeriod(timeZoneInfo);
                _cachedTimeZone = timeZoneInfo.Id;
            }
            
            IList<IPersonPeriod> personPeriods = obj.PersonPeriods(_cachedDateOnlyPeriod.Value);
            foreach (IPersonPeriod personPeriod in personPeriods)
            {
                if (_teamCollection.Contains(personPeriod.Team))
                {
                    return true;
                }
            }
            return false;
        }
    }
}