using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Business Unit.
    /// </summary>
    public class PersonBelongsToBusinessUnitSpecification : Specification<IPerson>
    {
        private readonly BusinessUnit _businessUnit;
        private readonly DateOnlyPeriod _dateTimePeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToBusinessUnitSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="businessUnit">The business unit.</param>
        public PersonBelongsToBusinessUnitSpecification(DateOnlyPeriod dateTimePeriod, BusinessUnit businessUnit)
        {
            _businessUnit = businessUnit;
            _dateTimePeriod = dateTimePeriod;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if person is member of the specified Business Unit; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            return (new PersonBelongsToSiteSpecification(_dateTimePeriod, _businessUnit.SiteCollection).IsSatisfiedBy(obj));
        }
    }
}