﻿using System.Collections.Generic;
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

			var ret = islands.ToList();
			while (true)
			{
				var island1 = ret.First();
				if (island1.AgentsInIsland().Count() <= _mergeIslandsSizeLimit.Limit)
				{
					var island2 = ret.Last();
					if (island1 == island2)
						return ret;
					var newIsland = Island.Merge(island1, island2);
					ret.Add(newIsland);
					ret.Remove(island1);
					ret.Remove(island2);
				}
				else
				{
					return ret;
				}
			}
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