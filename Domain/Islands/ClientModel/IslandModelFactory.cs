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
			var islandsBefore = _createIslands.Create(new ReduceNoSkillGroups(), agents, period);
			var islandsAfter = _createIslands.Create(_reduceSkillGroups, agents, period);
			var islandTopModel = new IslandTopModel
			{
				AfterReducing = from Island island in islandsAfter select island.CreateClientModel(),
				BeforeReducing = from Island island in islandsBefore select island.CreateClientModel()
			};

			return islandTopModel;

		}
	}
}