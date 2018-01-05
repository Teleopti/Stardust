using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Islands
{
	public class MergeIslandsSizeLimit
	{
		[RemoveMeWithToggle("Seal this", Toggles.ResourcePlanner_NoPytteIslands_47500)]
		public virtual int MaximumNumberOfAgentsInIsland  { get; private set; } = 50;
		
		public void SetValues_UseOnlyFromTest(int size)
		{
			MaximumNumberOfAgentsInIsland = size;
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_NoPytteIslands_47500)]
	public class NeverMergeIslands : MergeIslandsSizeLimit
	{
		public override int MaximumNumberOfAgentsInIsland => 0;
	}
}