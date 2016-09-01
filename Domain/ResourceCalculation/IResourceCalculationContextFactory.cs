using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public interface IResourceCalculationContextFactory
	{
		IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills);
		IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period);
	}
}