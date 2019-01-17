using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
	[TestFixture]
	public class JobListTest
	{
		private const string insightsJob = "Insights data refresh";

		[Test]
		public void VerifyGetJobList()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			Assert.Greater(new JobCollection(jobParameters).Count, 0);
		}
		
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void InsightsJobShouldDependsOnLicensedAndToggle(bool insightsEnabled, bool toggleEnabled)
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(insightsEnabled);

			if (toggleEnabled)
			{
				((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
					Toggles.WFM_Insights_80704);
			}

			var licenseActivator = new FakeLicenseActivator("test customer");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiWfmInsights);
			DefinedLicenseDataFactory.SetLicenseActivator(UnitOfWorkFactory.Current.Name, licenseActivator);

			var expectedResult = insightsEnabled & toggleEnabled;
			new JobCollection(jobParameters).Any(x=>x.Name == insightsJob).Should().Be.EqualTo(expectedResult);
		}
	}
}
