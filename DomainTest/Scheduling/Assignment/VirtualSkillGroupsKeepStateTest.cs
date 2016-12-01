﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.Legacy;
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

			var uniqueResults = new ConcurrentDictionary<VirtualSkillGroupsCreatorResult, byte>();
			Parallel.For(0, numberOfThreads, i =>
			{
				using (Context.Create(period))
				{
					uniqueResults.GetOrAdd(Target.Fetch(), result => { return 0; });
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
			using (Context.Create(new DateOnlyPeriod(2000,1,1,2000,1,2)))
			{
				Assert.Throws<NotSupportedException>(() =>
				{
					using (Context.Create(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2)))
					{
					}
				});
			}
		}
	}
}