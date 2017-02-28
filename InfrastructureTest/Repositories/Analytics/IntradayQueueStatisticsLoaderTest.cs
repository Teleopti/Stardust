using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class IntradayQueueStatisticsLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredProcedureExists()
		{
			var target = new IntradayQueueStatisticsLoader();
			var skill = SkillFactory.CreateSkillWithId("skill"); 
			var actualWorkloadInSecondsPerSkillInterval = target.LoadActualCallPerSkillInterval(new List<ISkill>() { skill }, TimeZoneInfo.Utc, DateOnly.Today);

			actualWorkloadInSecondsPerSkillInterval.Count.Should().Be.EqualTo(0);
		}
	}
}