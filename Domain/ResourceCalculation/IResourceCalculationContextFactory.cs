using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculationContextFactory
	{
		IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills);
		IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period);
	}
}