using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public interface ISkillGroupContext
	{
		IDisposable Create(DateOnlyPeriod period);
	}
}