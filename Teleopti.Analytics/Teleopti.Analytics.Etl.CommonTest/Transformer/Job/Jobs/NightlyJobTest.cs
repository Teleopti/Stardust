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
	[Ignore("TODO-xinfli: Flaky")]
	[TestFixture]
	public class NightlyJobTest
	{
		private const string insightsJobStepName = "Trigger Insights data refresh";

		[Test]
		public void ShouldIncludeInsightsJobStepOnlyIfLicensedAndToggleEnabled()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);

			((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
				Toggles.WFM_Insights_78059);

			var licenseActivator = new FakeLicenseActivator("test customer");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiWfmInsights);
			DefinedLicenseDataFactory.SetLicenseActivator(UnitOfWorkFactory.Current.Name, licenseActivator);

			new NightlyJobCollection(jobParameters).Any(x=>x.Name == insightsJobStepName).Should().Be.True();
		}

		[Test]
		public void ShouldExcludeInsightsJobIfNotLicensed()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			((JobParametersFactory.FakeContainerHolder) jobParameters.ContainerHolder).EnableToggle(
				Toggles.WFM_Insights_78059);
			
			new NightlyJobCollection(jobParameters).Any(x=>x.Name == insightsJobStepName).Should().Be.False();
		}

		[Test]
		public void ShouldExcludeInsightsJobOnlyIfToggleNotEnabled()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);

			var licenseActivator = new FakeLicenseActivator("test customer");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiWfmInsights);
			DefinedLicenseDataFactory.SetLicenseActivator(UnitOfWorkFactory.Current.Name, licenseActivator);
			
			new NightlyJobCollection(jobParameters).Any(x=>x.Name == insightsJobStepName).Should().Be.False();
		}
	}
}