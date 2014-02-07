using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class StatisticsUpdateNotificationJobStepTest
	{
		private IMessageSender _messageSender;
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_messageSender = MockRepository.GenerateMock<IMessageSender>();

			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), _messageSender, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldSendMessageBrokerEvent()
		{
			_messageSender.Expect(x => x.IsAlive).Return(false).Repeat.Once();
			_messageSender.Expect(x => x.StartBrokerService());
			_messageSender.Expect(x => x.IsAlive).Return(true).Repeat.Once();
			_messageSender.Expect(
				x =>
				x.SendData(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<Guid>.Is.Anything,
						   Arg<Guid>.Matches(g2 => g2 == Guid.Empty), Arg<Type>.Matches(t => t == typeof(IStatisticTask)), Arg<string>.Is.Anything,
				           Arg<Guid>.Is.Anything));

			var target = new StatisticsUpdateNotificationJobStep(_jobParameters);
			IJobStepResult jobStepResult = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			jobStepResult.Status.Should().Be.EqualTo("Done");
			_messageSender.VerifyAllExpectations();
		}
	}
}
