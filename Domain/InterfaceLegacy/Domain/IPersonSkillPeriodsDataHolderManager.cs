using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPersonSkillPeriodsDataHolderManager
    {
	    /// <summary>
	    /// Gets the person skill periods data holder dictionary.
	    /// </summary>
	    /// <param name="personSkillDay"></param>
	    /// <returns></returns>
		IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(PersonSkillDay personSkillDay);

	    /// <summary>
	    /// Gets the person NonBlendSkill SkillStaffPeriods.
	    /// </summary>
	    /// <param name="personSkillDay"></param>
	    /// <returns></returns>
	    IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(PersonSkillDay personSkillDay);
    }
}
