using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillsToSkillTest
	{
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private ExportMultisiteSkillsToSkillConsumer target;
		private MockRepository mocks;
		private IJobResultRepository jobResultRepository;
		private IJobResultFeedback jobResultFeedback;
		private IMessageBroker messageBroker;
		private IServiceBus serviceBus;
		private readonly Guid firstMultisiteId = Guid.NewGuid();
		private readonly Guid secondMultisiteId = Guid.NewGuid();
		private DateOnlyPeriod period = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 31);

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			jobResultRepository = mocks.DynamicMock<IJobResultRepository>();
			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			messageBroker = mocks.DynamicMock<IMessageBroker>();
			serviceBus = mocks.DynamicMock<IServiceBus>();

			target = new ExportMultisiteSkillsToSkillConsumer(unitOfWorkFactory, jobResultRepository, jobResultFeedback, messageBroker, serviceBus);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var jobResult = mocks.DynamicMock<IJobResult>();
			var jobId = Guid.NewGuid();
			using (mocks.Record())
			{
				var uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(jobResult.FinishedOk).Return(false);
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
				Expect.Call(() => jobResultFeedback.SetJobResult(jobResult, messageBroker));
			}
			using (mocks.Playback())
			{
				var message = new ExportMultisiteSkillsToSkill { JobId = jobId };
				message.Period = period;
				message.MultisiteSkillSelections.Add(new MultisiteSkillSelection { MultisiteSkillId = firstMultisiteId });
				message.MultisiteSkillSelections.Add(new MultisiteSkillSelection { MultisiteSkillId = secondMultisiteId });
				target.Consume(message);
			}
		}

		[Test]
		public void ShouldIgnoreAlreadyHandledMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var jobResult = mocks.DynamicMock<IJobResult>();
			var jobId = Guid.NewGuid();
			using (mocks.Record())
			{
				var uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(jobResult.FinishedOk).Return(true);
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
			}
			using (mocks.Playback())
			{
				var message = new ExportMultisiteSkillsToSkill { JobId = jobId };
				target.Consume(message);
			}
		}
	}
}
