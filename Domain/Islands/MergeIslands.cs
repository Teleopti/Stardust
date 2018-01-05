using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Islands
{
	public class MergeIslands
	{
		private readonly MergeIslandsSizeLimit _mergeIslandsSizeLimit;

		public MergeIslands(MergeIslandsSizeLimit mergeIslandsSizeLimit)
		{
			_mergeIslandsSizeLimit = mergeIslandsSizeLimit;
		}
		
		[RemoveMeWithToggle("Seal this", Toggles.ResourcePlanner_NoPytteIslands_47500)]
		public virtual IEnumerable<Island> Execute(IEnumerable<Island> islands)
		{
			if (!islands.Any())
				return islands;
			if (islands.First().AgentsInIsland().Count() <= _mergeIslandsSizeLimit.Limit)
			{
				var newIslands = islands.ToList();
				var newIsland = Island.Merge(islands.First(), islands.Last());
				newIslands.Add(newIsland);
				newIslands.Remove(islands.First());
				newIslands.Remove(islands.Last());
				return newIslands;
			}
			return islands;
		}

	}

	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_NoPytteIslands_47500)]
	public class NeverMergeIslands : MergeIslands
	{
		public override IEnumerable<Island> Execute(IEnumerable<Island> islands)
		{
			return islands;
		}

		public NeverMergeIslands() : base(null)
		{
		}
	}
}