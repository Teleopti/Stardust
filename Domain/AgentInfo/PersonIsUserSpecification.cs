using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Persons who are users. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// A person is a user if fullfills the following specifications:
    /// /// <list type="bullet">
    /// 	<item>
    /// 		<description>if has got a valid person period in the specific query time</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>AND has no terminal date OR the terminal date is later than the query time</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class PersonIsUserSpecification : Specification<IPerson>
    {
        private readonly DateOnlyPeriod _queryDatePeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToAnyBusinessUnitSpecification"/> class.
        /// </summary>
        /// <param name="queryDate">The date time period.</param>
        public PersonIsUserSpecification(DateOnly queryDate)
        {
            _queryDatePeriod = new DateOnlyPeriod(queryDate,queryDate);
        }

        public PersonIsUserSpecification(DateOnlyPeriod queryDatePeriod)
        {
            _queryDatePeriod = queryDatePeriod;
        }

        /// <summary>
        /// Determines whether the person is a user only.
        /// </summary>
        /// <param name="obj">The person.</param>
        /// <returns>
        /// 	<c>true</c> if person is user only; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            if (obj.PersonPeriodCollection.Count > 0) return false;
            return !(obj.TerminalDate.GetValueOrDefault(_queryDatePeriod.EndDate) < _queryDatePeriod.EndDate);
        }
    }
}