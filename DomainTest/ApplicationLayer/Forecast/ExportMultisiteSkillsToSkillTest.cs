using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillsToSkillTest
	{
		private ICurrentUnitOfWork unitOfWorkFactory;
		private ExportMultisiteSkillsEventHandler target;
		private MockRepository mocks;
		private IJobResultRepository jobResultRepository;
		private IJobResultFeedback jobResultFeedback;
		private IMessageBrokerComposite messageBroker;
		private readonly Guid firstMultisiteId = Guid.NewGuid();
		private readonly Guid secondMultisiteId = Guid.NewGuid();
		private DateOnlyPeriod period = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 31);
		private IExportMultisiteSkillProcessor _exportMultisiteSkillProcessor;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWork>();
			jobResultRepository = mocks.DynamicMock<IJobResultRepository>();
			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			messageBroker = mocks.DynamicMock<IMessageBrokerComposite>();
			_exportMultisiteSkillProcessor = mocks.DynamicMock<IExportMultisiteSkillProcessor>();

			target = new ExportMultisiteSkillsEventHandler(unitOfWorkFactory, jobResultRepository, jobResultFeedback, messageBroker,_exportMultisiteSkillProcessor);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var jobResult = mocks.DynamicMock<IJobResult>();
			var jobId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.Current()).Return(unitOfWork);
				Expect.Call(jobResult.FinishedOk).Return(false);
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
				Expect.Call(() => jobResultFeedback.SetJobResult(jobResult, messageBroker));
			}
			using (mocks.Playback())
			{
				var message = new ExportMultisiteSkillsToSkillEvent { JobId = jobId };
				message.PeriodStart = period.StartDate.Date;
				message.PeriodEnd = period.EndDate.Date;
				message.MultisiteSkillSelections.Add(new MultisiteSkillSelection { MultisiteSkillId = firstMultisiteId });
				message.MultisiteSkillSelections.Add(new MultisiteSkillSelection { MultisiteSkillId = secondMultisiteId });
				target.Handle(message);
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
				Expect.Call(unitOfWorkFactory.Current()).Return(unitOfWork);
				Expect.Call(jobResult.FinishedOk).Return(true);
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
			}
			using (mocks.Playback())
			{
				var message = new ExportMultisiteSkillsToSkillEvent { JobId = jobId };
				target.Handle(message);
			}
		}
	}
}
