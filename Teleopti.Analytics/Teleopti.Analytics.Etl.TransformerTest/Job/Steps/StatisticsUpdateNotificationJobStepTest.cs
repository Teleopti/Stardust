using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
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
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), _messageSender, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldInitiateMessageBrokerServiceWhenNotAlive()
		{
			_messageSender.Expect(x => x.IsAlive).Return(false).Repeat.Once();
			_messageSender.Expect(x => x.InstantiateBrokerService());
			_messageSender.Expect(x => x.IsAlive).Return(true).Repeat.Once();
			_messageSender.Expect(
				x =>
				x.SendData(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<Guid>.Is.Anything,
						   Arg<Guid>.Matches(g2 => g2 == Guid.Empty), Arg<Type>.Matches(t => t == typeof(IStatisticTask)),
						   Arg<DomainUpdateType>.Matches(d => d == DomainUpdateType.Insert), Arg<string>.Is.Anything,
				           Arg<Guid>.Is.Anything));

			var target = new StatisticsUpdateNotificationJobStep(_jobParameters);
			IJobStepResult jobStepResult = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			jobStepResult.Status.Should().Be.EqualTo("Done");
			_messageSender.VerifyAllExpectations();
		}
	}
}
