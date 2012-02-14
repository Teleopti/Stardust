using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Persons who do not belong to the hierarchy, that is do not belong to any 
    /// business units.
    /// </summary>
    public class PersonBelongsToAnyBusinessUnitSpecification : Specification<IPerson>
    {
        private readonly DateTimePeriod _dateTimePeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToAnyBusinessUnitSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        public PersonBelongsToAnyBusinessUnitSpecification(DateTimePeriod dateTimePeriod)
        {
            _dateTimePeriod = dateTimePeriod;
        }

        /// <summary>
        /// Determines whether the person belongs to the organization.
        /// </summary>
        /// <param name="obj">The person.</param>
        /// <returns>
        /// 	<c>true</c> if person is member of the organization; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_dateTimePeriod.StartDateTime), new DateOnly(_dateTimePeriod.EndDateTime));
            return (obj.PersonPeriods(dateOnlyPeriod).Count > 0);
        }
    }
}