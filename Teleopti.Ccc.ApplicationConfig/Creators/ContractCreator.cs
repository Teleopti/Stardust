using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class ContractCreator
    {
        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="employmentType">Type of the employment.</param>
        /// <param name="workTime">The work time.</param>
        /// <param name="workTimeDirective">The work time directive.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-11
        /// </remarks>
        public IContract Create(string name, Description description, EmploymentType employmentType, WorkTime workTime, WorkTimeDirective workTimeDirective)
        {
            IContract contract = new Contract(name)
            {
                Description = description,
                EmploymentType = employmentType,
                WorkTime = workTime,
                WorkTimeDirective = workTimeDirective
            };
            return contract;
        }
    }
}
