using System;
using System.IO;
using System.Threading;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();
			TestSiteConfigurationSetup.Setup(true);

			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader());
			var fakeToggleManager = new FakeToggleManager();
			var configuration = new IocConfiguration(args, fakeToggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.Build();
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}

		public static void LogHangfireQueues(TestLog testLog, HangfireUtilities hangfireUtilities)
		{
			while (true)
			{
				testLog.Debug($"Hangfire is processing {hangfireUtilities.NumberOfProcessingJobs()} jobs, {hangfireUtilities.NumberOfScheduledJobs()} are scheduled and {hangfireUtilities.NumberOfFailedJobs()} jobs has failed, {hangfireUtilities.SucceededFromStatistics()} jobs has succeeded.");
				foreach (var queueName in Queues.OrderOfPriority())
				{
					testLog.Debug($"{hangfireUtilities.NumberOfJobsInQueue(queueName)} jobs in queue '{queueName}'");
				}
				Thread.Sleep(TimeSpan.FromSeconds(60));
			}
		}
	}
}