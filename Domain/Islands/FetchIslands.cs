using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class FetchIslands
	{
		private readonly CreateIslands _createIslands;

		public FetchIslands(CreateIslands createIslands)
		{
			_createIslands = createIslands;
		}
		
		public IEnumerable<Island> Execute(DateOnlyPeriod period)
		{
			return _createIslands.Create(period, null);
		}
	}
}