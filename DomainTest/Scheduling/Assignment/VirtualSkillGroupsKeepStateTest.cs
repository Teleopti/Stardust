using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class VirtualSkillGroupsKeepStateTest
	{
		public VirtualSkillGroupsResultProvider Target;
		public VirtualSkillContext Context;

		[Test]
		public void ShouldKeepResultInternally()
		{
			using (Context.Create(new DateOnlyPeriod()))
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

			var results = new HashSet<VirtualSkillGroupsCreatorResult>();
			var tasks = new List<Task>(10);
			for (var i = 0; i < numberOfThreads; i++)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					using (Context.Create(period))
					{
						results.Add(Target.Fetch());
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());

			results.Count.Should().Be.EqualTo(numberOfThreads);
		}

		[Test]
		public void CannotFetchResultBeforeContextIsCreated()
		{
			Assert.Throws<NotSupportedException>(() => Target.Fetch());
		}

		[Test]
		public void CannotCreateNestedContexts()
		{
			using (Context.Create(new DateOnlyPeriod(2000,1,1,2000,1,2)))
			{
				Assert.Throws<NotSupportedException>(() => Context.Create(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2)));
			}
		}
	}
}