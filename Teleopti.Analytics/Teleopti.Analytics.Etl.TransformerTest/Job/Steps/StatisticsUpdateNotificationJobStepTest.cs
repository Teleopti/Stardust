using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class StatisticsUpdateNotificationJobStepTest
	{
		[Test]
		public void ShouldSendMessageBrokerEvent()
		{
			var messageClient = MockRepository.GenerateMock<ISignalRClient>();
			var messageSender = MockRepository.GenerateMock<IMessageSender>();

			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), messageClient, messageSender, null);
			
			messageClient.Stub(x => x.IsAlive).Return(true);

			var target = new StatisticsUpdateNotificationJobStep(jobParameters);
			IJobStepResult jobStepResult = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			var arguments = messageSender.GetArgumentsForCallsMadeOn(x => x.Send((Notification) null), a => a.IgnoreArguments());

			jobStepResult.Status.Should().Be.EqualTo("Done");

			var firstCall = arguments.Single();
			var notification = (Notification)firstCall.Single();
			notification.DomainId.Should().Be(Guid.Empty.ToString());
			notification.DomainType.Should().Be(typeof(IStatisticTask).Name);
			
			messageSender.VerifyAllExpectations();
		}
	}
}
