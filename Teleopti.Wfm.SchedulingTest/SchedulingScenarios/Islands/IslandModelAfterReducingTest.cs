using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands
{
	[DomainTest]
	public class IslandModelAfterReducingTest
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		[Test]
		public void ShouldNotLeaveDuplicateSkillSetsAfterReducing()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			Enumerable.Range(0, 3).Select(x => new Person().WithPersonPeriod(skillA))
				.ForEach(x => PersonRepository.Has(x));
			PersonRepository.Has(new Person().WithPersonPeriod(skillA, skillB));
			PersonRepository.Has(new Person().WithPersonPeriod(skillB));

			var model = IslandModelFactory.Create();
			model.MoreIslandsBySkillReducing.Islands.All(x => x.SkillSets.Count() == 1) //two islands with one skillgroup each
				.Should().Be.True();
		}

		[TestCase(100)]
		[TestCase(0)]
		public void ReducingShouldNotAffectBasicIslandNumberOfAgentsOnAllIslandsResult(int minAgentsInIslandLimit)
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(minAgentsInIslandLimit, 2);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			Enumerable.Range(0, 3).Select(x => new Person().WithPersonPeriod(skillA))
				.ForEach(x => PersonRepository.Has(x));
			PersonRepository.Has(new Person().WithPersonPeriod(skillA, skillB));
			PersonRepository.Has(new Person().WithPersonPeriod(skillB));

			var model = IslandModelFactory.Create();

			model.BasicIslands.NumberOfAgentsOnAllIslands
				.Should().Be.EqualTo(5);
		}
	}
}