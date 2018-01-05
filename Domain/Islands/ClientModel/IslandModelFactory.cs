using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModelFactory
	{
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillSets _reduceSkillSets;
		private readonly IAllStaff _allStaff;

		public IslandModelFactory(CreateIslands createIslands, ReduceSkillSets reduceSkillSets, IAllStaff allStaff)
		{
			_createIslands = createIslands;
			_reduceSkillSets = reduceSkillSets;
			_allStaff = allStaff;
		}

		public IslandTopModel Create()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var agents = _allStaff.Agents(period);
			var createIslandsCallback = new LogCreateIslandsCallback();
			_createIslands.Create(_reduceSkillSets, agents, period, createIslandsCallback);

			var islandTopModel = new IslandTopModel
			{
				BeforeReducing = createIslandModel(createIslandsCallback.IslandsBasic),
				AfterReducing = createIslandModel(createIslandsCallback.IslandsAfterReducing),
			};

			return islandTopModel;
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
			islandsModel.NumberOfAgentsOnAllIsland = islandsModel.Islands.Sum(x => x.NumberOfAgentsOnIsland);
			return islandsModel;
		}
	}
}