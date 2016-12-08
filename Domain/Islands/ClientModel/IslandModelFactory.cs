using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModelFactory
	{
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public IslandModelFactory(CreateIslands createIslands, ReduceSkillGroups reduceSkillGroups, IPeopleInOrganization peopleInOrganization)
		{
			_createIslands = createIslands;
			_reduceSkillGroups = reduceSkillGroups;
			_peopleInOrganization = peopleInOrganization;
		}

		public IslandTopModel Create()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var agents = _peopleInOrganization.Agents(period);

			var islandsModelBefore = createIslandsModel(new ReduceNoSkillGroups(), agents, period);
			var islandsModelAfter = createIslandsModel(_reduceSkillGroups, agents, period);

			var islandTopModel = new IslandTopModel
			{
				AfterReducing = islandsModelAfter,
				BeforeReducing = islandsModelBefore
			};

			return islandTopModel;
		}

		private IslandsModel createIslandsModel(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var islands = _createIslands.Create(reduceSkillGroups, agents, period);
			var islandsModel = new IslandsModel
			{
				Islands = from Island island in islands select island.CreateClientModel()
			};
			islandsModel.NumberOfAgentsOnAllIsland = islandsModel.Islands.Sum(x => x.NumberOfAgentsOnIsland);
			return islandsModel;
		}
	}
}