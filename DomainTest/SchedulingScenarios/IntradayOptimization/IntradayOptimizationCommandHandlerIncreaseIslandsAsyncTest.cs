using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayOptimizationCommandHandlerIncreaseIslandsAsyncTest
	{
		public IntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		[Test]
		public void ShouldSetTotalEvents()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 4);
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillC = new Skill("C");
			var skillAagents21 = Enumerable.Range(0, 11).Select(x => new Person().WithPersonPeriod(skillA));
			var skillABagents5 = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillB));
			var skillACagents5 = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillC));
			skillAagents21.Union(skillABagents5).Union(skillACagents5).ForEach(x => PersonRepository.Has(x));

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod()});

			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>()
				.All(x => x.TotalEvents == 2)
				.Should()
				.Be.True();
		}
	}
}