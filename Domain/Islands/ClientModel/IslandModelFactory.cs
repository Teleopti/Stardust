using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModelFactory
	{
		private readonly CreateIslands _createIslands;

		public IslandModelFactory(CreateIslands createIslands)
		{
			_createIslands = createIslands;
		}

		public IEnumerable<IslandModel> Create()
		{
			var islands = _createIslands.Create(DateOnly.Today.ToDateOnlyPeriod());
			var ret = new List<IslandModel>();
			foreach (Island island in islands)
			{
				ret.Add(island.CreateClientModel());
			}
			return ret;
		}
	}
}