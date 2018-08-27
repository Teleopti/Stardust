using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[AnalyticsDatabaseTest]
	public class AnalyticsScheduleMatchingPersonTest
	{ 
		public AnalyticsScheduleMatchingPerson Target;

		[Test]
		public void ShouldNotBlowUpWhenPersonDoesNotExist()
		{
			var newGuid = Guid.NewGuid();

			Assert.DoesNotThrow(() =>
			{
				Target.Handle(new AnalyticsPersonPeriodRangeChangedEvent
				{
					PersonIdCollection = { newGuid }
				});
			});
		}
	}
}