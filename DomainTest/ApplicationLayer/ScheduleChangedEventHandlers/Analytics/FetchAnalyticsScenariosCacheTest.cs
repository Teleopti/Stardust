using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[DomainTest]
	public class FetchAnalyticsScenariosCacheTest
	{
		public FetchAnalyticsScenarios Target;
		public FakeAnalyticsScenarioRepository AnalyticsScenarioRepository;
		
		[Test]
		public void ShouldCacheAnalyticsScenarios([Range(1, 5)]int numberOfCalls)
		{
			for (var i = 0; i < numberOfCalls; i++)
			{
				Target.Execute();
			}
			
			AnalyticsScenarioRepository.NumberOfScenariosReads
				.Should().Be.EqualTo(1);
		}
	}
}