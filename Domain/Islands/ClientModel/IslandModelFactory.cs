using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var createIslandsCallback = new LogCreateIslandsCallback();
			_createIslands.Create(period, createIslandsCallback);
			
			return new IslandTopModel
			{
				BasicIslands = createIslandModel(createIslandsCallback.IslandsBasic),
				MoreIslandsBySkillReducing = createIslandModel(createIslandsCallback.IslandsAfterReducing),
				FewerIslandsByMerging = createIslandModel(createIslandsCallback.IslandsComplete)
			};
		}

		private static IslandsModel createIslandModel(LogCreateIslandInfo logCreateIslandInfo)
		{
			var islandModel = logCreateIslandInfo.Islands.Select(x => x.CreateClientModel()).ToList();
			islandModel.Sort();
			var islandsModel = new IslandsModel
			{
				Islands = islandModel,
				TimeToGenerateInMs = (int) Math.Ceiling(logCreateIslandInfo.TimeToGenerate.TotalMilliseconds),
			};
			islandsModel.NumberOfAgentsOnAllIslands = islandsModel.Islands.Sum(x => x.NumberOfAgentsOnIsland);
			return islandsModel;
		}
	}
}