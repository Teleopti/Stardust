using Teleopti.Ccc.Domain.Common;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Restriction min/max occupancy during skill day or skill day templates
    /// </summary>
    public class MinMaxOccupancy : IRestriction<ISkillDay>, IRestriction<ISkill>
    {
        #region IRestriction<ISkillDay> Members

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(ISkillDay entityToCheck)
        {
            CheckSkillDataPeriodsForInvalidItems(entityToCheck.SkillDataPeriodCollection.OfType<ISkillData>());
        }

        #endregion

        private static void CheckSkillDataPeriodsForInvalidItems(IEnumerable<ISkillData> skillDataPeriodList)
        {
            bool hasInValidItems = skillDataPeriodList.Any(p => p.ServiceAgreement.MinOccupancy > p.ServiceAgreement.MaxOccupancy);
            if (hasInValidItems)
            {
                throw new ValidationException(string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.MaximumAgentOccupancyMustBeLargerThanOrEqualToMinimumAgentOccupancy));
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void CheckEntity(ISkill entityToCheck)
        {
            var templateSkillDataPeriodList = from t in entityToCheck.TemplateWeekCollection
                                              from tp in t.Value.TemplateSkillDataPeriodCollection
                                              select tp;

            CheckSkillDataPeriodsForInvalidItems(templateSkillDataPeriodList.OfType<ISkillData>());
        }

        #endregion
    }
}