using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModelFactory
	{
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillGroups _reduceSkillGroups;

		public IslandModelFactory(CreateIslands createIslands, ReduceSkillGroups reduceSkillGroups)
		{
			_createIslands = createIslands;
			_reduceSkillGroups = reduceSkillGroups;
		}

		public IslandTopModel Create()
		{
			var islandsBefore = _createIslands.Create(new ReduceNoSkillGroups(), DateOnly.Today.ToDateOnlyPeriod());
			var islandsAfter = _createIslands.Create(_reduceSkillGroups, DateOnly.Today.ToDateOnlyPeriod());
			var islandTopModel = new IslandTopModel
			{
				AfterReducing = from Island island in islandsAfter select island.CreateClientModel(),
				BeforeReducing = from Island island in islandsBefore select island.CreateClientModel()
			};

			return islandTopModel;

		}
	}
}