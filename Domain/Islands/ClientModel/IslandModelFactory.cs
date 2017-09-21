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
		private readonly IPeopleInOrganization _peopleInOrganization;

		public IslandModelFactory(CreateIslands createIslands, ReduceSkillSets reduceSkillSets, IPeopleInOrganization peopleInOrganization)
		{
			_createIslands = createIslands;
			_reduceSkillSets = reduceSkillSets;
			_peopleInOrganization = peopleInOrganization;
		}

		public IslandTopModel Create()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var agents = _peopleInOrganization.Agents(period);

			var islandsModelBefore = createIslandsModel(new ReduceNoSkillSets(), agents, period);
			var islandsModelAfter = createIslandsModel(_reduceSkillSets, agents, period);

			var islandTopModel = new IslandTopModel
			{
				AfterReducing = islandsModelAfter,
				BeforeReducing = islandsModelBefore
			};

			return islandTopModel;
		}

		private IslandsModel createIslandsModel(IReduceSkillSets reduceSkillSets, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			IEnumerable<Island> islands=null;
			var timeToGenerate = MeasureTime.Do(() => islands = _createIslands.Create(reduceSkillSets, agents, period));
			var islandModels = (from Island island in islands select island.CreateClientModel()).ToList();
			islandModels.Sort();
			var islandsModel = new IslandsModel
			{
				Islands = islandModels,
				TimeToGenerateInMs = (int) Math.Ceiling(timeToGenerate.TotalMilliseconds)
			};
			islandsModel.NumberOfAgentsOnAllIsland = islandsModel.Islands.Sum(x => x.NumberOfAgentsOnIsland);
			return islandsModel;
		}
	}
}