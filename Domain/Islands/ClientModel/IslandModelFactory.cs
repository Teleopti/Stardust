﻿using System.Linq;
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

		public IslandTopModel Create()
		{
			var islands = _createIslands.Create(DateOnly.Today.ToDateOnlyPeriod());
			var islandTopModel = new IslandTopModel();

			islandTopModel.AfterReducing = from Island island in islands select island.CreateClientModel();

			return islandTopModel;

		}
	}
}