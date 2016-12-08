using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Islands
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class IslandModelBeforeReducingTest
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		[Test, Ignore("Should be fixed")]
		public void ShouldNotReduce()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			Enumerable.Range(0, 3).Select(x => new Person().KnowsSkill(skillA))
				.ForEach(x => PersonRepository.Has(x));
			PersonRepository.Has(new Person().KnowsSkill(skillA, skillB));

			var model = IslandModelFactory.Create();
			model.BeforeReducing.Count()
				.Should().Be.EqualTo(1);
		}
	}
}