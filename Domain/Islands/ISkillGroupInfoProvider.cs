using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Islands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public interface ISkillGroupInfoProvider
	{
		ISkillGroupInfo Fetch();
	}
}