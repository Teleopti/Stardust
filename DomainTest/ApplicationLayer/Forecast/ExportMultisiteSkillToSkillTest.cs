using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillToSkillTest
	{
		private ICurrentUnitOfWork unitOfWorkFactory;
		private ISkillRepository skillRepository;
		private IMultisiteForecastToSkillCommand command;
		private ExportMultisiteSkillProcessor target;
		private MockRepository mocks;
		private IJobResultRepository jobResultRepository;
		private IJobResultFeedback jobResultFeedback;
		private IMessageBrokerComposite messageBroker;
		private IDisableBusinessUnitFilter _disableBusinessUnitFilter;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWork>();
			skillRepository = mocks.DynamicMock<ISkillRepository>();
			jobResultRepository = mocks.DynamicMock<IJobResultRepository>();
			skillRepository = mocks.DynamicMock<ISkillRepository>();
			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			messageBroker = mocks.DynamicMock<IMessageBrokerComposite>();
			command = mocks.DynamicMock<IMultisiteForecastToSkillCommand>();
			_disableBusinessUnitFilter = mocks.DynamicMock<IDisableBusinessUnitFilter>();
			target = new ExportMultisiteSkillProcessor(unitOfWorkFactory, skillRepository, jobResultRepository, command, jobResultFeedback, messageBroker, _disableBusinessUnitFilter);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
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
				Expect.Call(unitOfWorkFactory.Current()).Return(unitOfWork);
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
				target.Process(message);
			}
		}
	}
}