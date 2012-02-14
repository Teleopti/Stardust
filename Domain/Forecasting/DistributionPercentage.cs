using Teleopti.Ccc.Domain.Common;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Restriction percentage during multisite day or multisite skill day templates
    /// </summary>
    public class DistributionPercentage : IRestriction<IMultisiteDay>, IRestriction<ISkill>
    {
        #region IRestriction<MultisiteDay> Members

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(IMultisiteDay entityToCheck)
        {
            CheckMultisitePeriodsForInvalidItems(entityToCheck.MultisitePeriodCollection.OfType<IMultisiteData>());
        }

        #endregion

        private static void CheckMultisitePeriodsForInvalidItems(IEnumerable<IMultisiteData> multisitePeriodList)
        {
            IMultisiteData invalidItem = multisitePeriodList.FirstOrDefault(p => !p.IsValid);
            if (invalidItem!=null)
            {
                throw new ValidationException(
                    string.Format(CultureInfo.InvariantCulture, "The multisite periods contains items that are invalid.")); //TODO! Return date and time!
            }
        }

        #region IRestriction<Skill> Members

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(ISkill entityToCheck)
        {
            IMultisiteSkill multisiteSkill = entityToCheck as IMultisiteSkill;
            if (multisiteSkill == null) return;
            var templateMultisitePeriodList = from t in multisiteSkill.TemplateMultisiteWeekCollection
                                              from tp in t.Value.TemplateMultisitePeriodCollection
                                              select tp;

            CheckMultisitePeriodsForInvalidItems(templateMultisitePeriodList.OfType<IMultisiteData>());
        }

        #endregion
    }
}