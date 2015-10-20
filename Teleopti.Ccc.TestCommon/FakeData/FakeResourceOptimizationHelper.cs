using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeResourceOptimizationHelper : IResourceOptimizationHelper
	{
		public void ResourceCalculateDate(DateOnly localDate, bool considerShortBreaks)
		{
			throw new NotImplementedException();
		}

		public ISkillSkillStaffPeriodExtendedDictionary CreateSkillSkillStaffDictionaryOnSkills(
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary, IList<ISkill> skills, DateTimePeriod keyPeriod)
		{
			throw new NotImplementedException();
		}

		public void ResourceCalculateDate(IResourceCalculationDataContainer relevantProjections, DateOnly localDate, bool considerShortBreaks)
		{
			throw new NotImplementedException();
		}
	}
}