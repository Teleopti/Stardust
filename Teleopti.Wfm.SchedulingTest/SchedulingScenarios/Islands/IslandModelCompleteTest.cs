using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Islands
{
	[DomainTest]
	public class IslandModelCompleteTest
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldMergeSmallIslands()
		{
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			PersonRepository.Has(new Person().WithPersonPeriod(skillA));
			PersonRepository.Has(new Person().WithPersonPeriod(skillB));

			var model = IslandModelFactory.Create();
		
			var island = model.FewerIslandsByMerging.Islands.Single();
			island.NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
			island.SkillSets.Count().Should().Be.EqualTo(2);
		}
	}
}