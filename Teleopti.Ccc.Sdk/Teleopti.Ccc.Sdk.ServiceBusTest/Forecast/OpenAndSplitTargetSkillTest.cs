using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class OpenAndSplitTargetSkillTest
	{
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ISkillRepository _skillRepository;
		private IJobResultRepository _jobResultRepository;
		private IJobResultFeedback _feedback;
		private IMessageBrokerComposite _messageBroker;
		private IImportForecastsToSkillHandler _importForecastsToSkillHandler;
		private IOpenAndSplitSkillCommand _command;
		private OpenAndSplitTargetSkillHandler _target;
		private IUnitOfWork _unitOfWork;
		private IJobResult _jobResult;
		private IDisableBusinessUnitFilter _disableFilter;

		[SetUp]
		public void Setup()
		{
			_currentunitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_jobResultRepository = MockRepository.GenerateMock<IJobResultRepository>();
			_feedback = MockRepository.GenerateMock<IJobResultFeedback>();
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_importForecastsToSkillHandler = MockRepository.GenerateMock<IImportForecastsToSkillHandler>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_jobResult = MockRepository.GenerateMock<IJobResult>();
			_command = MockRepository.GenerateMock<IOpenAndSplitSkillCommand>();
			_disableFilter = MockRepository.GenerateMock<IDisableBusinessUnitFilter>();
			_target = new OpenAndSplitTargetSkillHandler(_currentunitOfWorkFactory, _command, _skillRepository,
				_jobResultRepository, _feedback, _messageBroker,
				_importForecastsToSkillHandler, _disableFilter);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var dateTime = new DateOnly(2012, 3, 1);
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			_command.Stub(x => x.Execute(skill, new DateOnlyPeriod(dateTime, dateTime), new[] { new TimePeriod(8, 0, 17, 0) }));
			_importForecastsToSkillHandler.Stub(x => x.Handle(null)).IgnoreArguments();

			var message = new OpenAndSplitTargetSkillEvent
			{
				JobId = jobId,
				ImportMode = ImportForecastsMode.ImportWorkload,
				TargetSkillId = skillId,
				Date = dateTime.Date,
				StartOpenHour = TimeSpan.FromHours(8),
				EndOpenHour = TimeSpan.FromHours(17),
				Timestamp = DateTime.Now
			};
			_target.Handle(message);
		}
	}
}
