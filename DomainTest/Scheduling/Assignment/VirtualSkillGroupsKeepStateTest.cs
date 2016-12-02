using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class VirtualSkillGroupsKeepStateTest : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerSplitBigIslands42049;
		public ISkillGroupInfoProvider Target;
		public ISkillGroupContext Context;

		public VirtualSkillGroupsKeepStateTest(bool resourcePlannerSplitBigIslands42049)
		{
			_resourcePlannerSplitBigIslands42049 = resourcePlannerSplitBigIslands42049;
		}

		[Test]
		public void ShouldKeepResultInternally()
		{
			using (Context.Create(Enumerable.Empty<IPerson>(), new DateOnlyPeriod()))
			{
				Target.Fetch()
					.Should().Be.SameInstanceAs(Target.Fetch());
			}
		}

		[Test]
		public void ShouldNotKeepResultBetweenThreads()
		{
			var period = new DateOnlyPeriod(2000, 1, 1, 2001, 1,1);
			const int numberOfThreads = 10;

			var uniqueResults = new ConcurrentDictionary<ISkillGroupInfo, byte>();
			Parallel.For(0, numberOfThreads, i =>
			{
				using (Context.Create(Enumerable.Empty<IPerson>(), period))
				{
					uniqueResults.GetOrAdd(Target.Fetch(), result => 0);
				}
			});

			uniqueResults.Count.Should().Be.EqualTo(numberOfThreads);
		}

		[Test]
		public void CannotFetchResultBeforeContextIsCreated()
		{
			Assert.Throws<NotSupportedException>(() => Target.Fetch());
		}

		[Test]
		public void CannotCreateNestedContexts()
		{
			using (Context.Create(Enumerable.Empty<IPerson>(), new DateOnlyPeriod(2000,1,1,2000,1,2)))
			{
				Assert.Throws<NotSupportedException>(() =>
				{
					using (Context.Create(Enumerable.Empty<IPerson>(), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2)))
					{
					}
				});
			}
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerSplitBigIslands42049)
				toggleManager.Enable(Toggles.ResourcePlanner_SplitBigIslands_42049);
		}
	}
}