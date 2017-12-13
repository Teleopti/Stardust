using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class FetchIslands
	{
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillSets _reduceSkillSets;
		private readonly IAllStaff _allStaff;

		public FetchIslands(CreateIslands createIslands,
			ReduceSkillSets reduceSkillSets,
			IAllStaff allStaff)
		{
			_createIslands = createIslands;
			_reduceSkillSets = reduceSkillSets;
			_allStaff = allStaff;
		}
		
		public IEnumerable<Island> Execute(DateOnlyPeriod period)
		{
			return _createIslands.Create(_reduceSkillSets, _allStaff.Agents(period), period);
		}
	}
}