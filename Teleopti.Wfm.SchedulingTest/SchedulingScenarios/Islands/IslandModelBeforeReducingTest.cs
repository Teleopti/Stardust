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
	public class IslandModelBeforeReducingTest
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		[Test]
		public void ShouldNotReduce()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			Enumerable.Range(0, 3).Select(x => new Person().WithPersonPeriod(skillA))
				.ForEach(x => PersonRepository.Has(x));
			PersonRepository.Has(new Person().WithPersonPeriod(skillA, skillB));

			var model = IslandModelFactory.Create();
			model.BasicIslands.Islands.Count()
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldKeepOldNumberOfAgentsOnSkill()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			Enumerable.Range(0, 3).Select(x => new Person().WithPersonPeriod(skillA))
				.ForEach(x => PersonRepository.Has(x));
			PersonRepository.Has(new Person().WithPersonPeriod(skillA, skillB));
			PersonRepository.Has(new Person().WithPersonPeriod(skillB));

			var model = IslandModelFactory.Create();
			foreach (var island in model.BasicIslands.Islands)
			{
				foreach (var skillSet in island.SkillSets)
				{
					var skillModel = skillSet.Skills.SingleOrDefault(x => x.Name.Equals(skillA.Name));
					if (skillModel != null)
					{
						skillModel.NumberOfAgentsOnSkill.Should().Be.EqualTo(4);
						return;
					}
				}
			}
			Assert.Fail("SkillA is gone!");
		}
	}
}