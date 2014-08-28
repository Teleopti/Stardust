using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillToSkillTest
	{
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private ISkillRepository skillRepository;
		private IMultisiteForecastToSkillCommand command;
		private ExportMultisiteSkillToSkillConsumer target;
		private MockRepository mocks;
		private IJobResultRepository jobResultRepository;
		private IJobResultFeedback jobResultFeedback;
		private IMessageBroker messageBroker;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			skillRepository = mocks.DynamicMock<ISkillRepository>();
			jobResultRepository = mocks.DynamicMock<IJobResultRepository>();
			skillRepository = mocks.DynamicMock<ISkillRepository>();
			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			messageBroker = mocks.DynamicMock<IMessageBroker>();
			command = mocks.DynamicMock<IMultisiteForecastToSkillCommand>();
			target = new ExportMultisiteSkillToSkillConsumer(unitOfWorkFactory, skillRepository, jobResultRepository, command, jobResultFeedback, messageBroker);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var disposable = mocks.DynamicMock<IDisposable>();
			var jobResult = mocks.DynamicMock<IJobResult>();
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("test");
			var childSkill = SkillFactory.CreateChildSkill("test", multisiteSkill);
			var skill = SkillFactory.CreateSkill("test2");
			var multisiteSkillId = Guid.NewGuid();
			var childSkillId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var jobId = Guid.NewGuid();
			using (mocks.Record())
			{
				var uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(unitOfWork.DisableFilter(QueryFilter.BusinessUnit)).Return(disposable);
				Expect.Call(() => command.Execute(null)).IgnoreArguments();
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
				Expect.Call(() => jobResultFeedback.SetJobResult(jobResult, messageBroker));
				Expect.Call(skillRepository.Get(multisiteSkillId)).Return(multisiteSkill);
				Expect.Call(skillRepository.Get(childSkillId)).Return(childSkill);
				Expect.Call(skillRepository.Get(skillId)).Return(skill);
			}
			using (mocks.Playback())
			{
				var message = new ExportMultisiteSkillToSkill { JobId = jobId };
				message.MultisiteSkillSelections = new MultisiteSkillSelection { MultisiteSkillId = multisiteSkillId };
				message.MultisiteSkillSelections.ChildSkillSelections.Add(new ChildSkillSelection { SourceSkillId = childSkillId, TargetSkillId = skillId });
				target.Consume(message);
			}
		}
	}
}