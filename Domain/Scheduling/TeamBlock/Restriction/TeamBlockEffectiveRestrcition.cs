using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class TeamBlockEffectiveRestrcition : IScheduleRestrictionStrategy
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IGroupPerson _groupPerson;
        private readonly IScheduleDictionary _scheduleDictionary;
        private readonly ISchedulingOptions _schedulingOptions;

        public TeamBlockEffectiveRestrcition(IEffectiveRestrictionCreator effectiveRestrictionCreator,
                                             IGroupPerson groupPerson, ISchedulingOptions schedulingOptions,
                                             IScheduleDictionary scheduleDictionary)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _groupPerson = groupPerson;
            _schedulingOptions = schedulingOptions;
            _scheduleDictionary = scheduleDictionary;
        }

        /// <summary>
        /// </summary>
        /// <param name="dateOnlyList"></param>
        /// <param name="matrixList">Pass null as no matrix list is used here</param>
        /// <returns></returns>
        public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList,
                                                        IList<IScheduleMatrixPro> matrixList)
        {
            IEffectiveRestriction effectiveRestriction = null;
            foreach (DateOnly dateOnly in dateOnlyList)
            {
                IEffectiveRestriction restriction =
                    _effectiveRestrictionCreator.GetEffectiveRestriction(_groupPerson.GroupMembers,
                                                                         dateOnly, _schedulingOptions,
                                                                         _scheduleDictionary);
                if (restriction == null)
                    return null;
                if (effectiveRestriction != null)
                    effectiveRestriction = effectiveRestriction.Combine(restriction);
                else
                    effectiveRestriction = restriction;
                if (effectiveRestriction == null)
                    return null;
            }
            return effectiveRestriction;
        }
    }
}