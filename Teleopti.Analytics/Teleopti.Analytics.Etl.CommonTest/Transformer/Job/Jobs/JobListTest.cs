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
		
		[Ignore("TODO-xinfli: Flaky")]
		[Test]
		public void ShouldIncludeInsightsJobOnlyIfLicensedAndToggleEnabled()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);

			((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
				Toggles.WFM_Insights_78059);

			var licenseActivator = new FakeLicenseActivator("test customer");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiWfmInsights);
			DefinedLicenseDataFactory.SetLicenseActivator(UnitOfWorkFactory.Current.Name, licenseActivator);

			new JobCollection(jobParameters).Any(x=>x.Name == insightsJob).Should().Be.True();
		}
		
		[Ignore("TODO-xinfli: Flaky")]
		[Test]
		public void ShouldExcludeInsightsJobIfNotLicensed()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
				Toggles.WFM_Insights_78059);
			
			new JobCollection(jobParameters).Any(x=>x.Name == insightsJob).Should().Be.False();
		}
		
		[Ignore("TODO-xinfli: Flaky")]
		[Test]
		public void ShouldExcludeInsightsJobOnlyIfToggleNotEnabled()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);

			var licenseActivator = new FakeLicenseActivator("test customer");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiWfmInsights);
			DefinedLicenseDataFactory.SetLicenseActivator(UnitOfWorkFactory.Current.Name, licenseActivator);

			new JobCollection(jobParameters).Any(x=>x.Name == insightsJob).Should().Be.False();
		}
	}
}
