using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
	[TestFixture]
	public class NightlyJobTest
	{
		private const string insightsJobStepName = "Trigger Insights data refresh";

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void InsightsTaskShouldDependsLicenseAndConfiguration(bool insightsEnabled, bool toggleEnabled)
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(insightsEnabled);

			if (toggleEnabled)
			{
				((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
					Toggles.WFM_Insights_80704);
			}

			var expectedResult = insightsEnabled && toggleEnabled;
			new NightlyJobCollection(jobParameters).Any(x => x.Name == insightsJobStepName).Should().Be
				.EqualTo(expectedResult);
		}
	}
}