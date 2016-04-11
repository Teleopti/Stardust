using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class ImportForecastsToSkillTest
	{
		private ImportForecastsToSkillHandler _target;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private ISkillRepository _skillRepository;
		private IJobResultRepository _jobResultRepository;
		private IJobResultFeedback _feedback;
		private IMessageBrokerComposite _messageBroker;
		private ISaveForecastToSkillCommand _saveForecastToSkillCommand;
		private IUnitOfWork _unitOfWork;
		private IJobResult _jobResult;
		private IDisableBusinessUnitFilter _disableFilter;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_jobResultRepository = MockRepository.GenerateMock<IJobResultRepository>();
			_feedback = MockRepository.GenerateMock<IJobResultFeedback>();
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_jobResult = MockRepository.GenerateMock<IJobResult>();
			_disableFilter = MockRepository.GenerateMock<IDisableBusinessUnitFilter>();
			_saveForecastToSkillCommand = MockRepository.GenerateMock<ISaveForecastToSkillCommand>();
			_target = new ImportForecastsToSkillHandler(_unitOfWorkFactory, _saveForecastToSkillCommand,
																		_skillRepository, _jobResultRepository, _feedback,
																		_messageBroker, _disableFilter);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var dateTime = new DateOnly(2012, 3, 1);
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			var row = new ForecastsRow
			{
				TaskTime = 179,
				AfterTaskTime = 0,
				Agents = 4.05,
				LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0),
				LocalDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0),
				SkillName = "Insurance",
				Tasks = 17,
				UtcDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc),
				UtcDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)
			};

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			_saveForecastToSkillCommand.Stub(x => x.Execute(dateTime, skill, new[] { row }, ImportForecastsMode.ImportWorkload));

			var message = new ImportForecastsToSkillEvent
			{
				JobId = jobId,
				ImportMode = ImportForecastsMode.ImportWorkload,
				TargetSkillId = skillId,
				Date = dateTime.Date,
				Forecasts = new[] { row },
				Timestamp = DateTime.Now
			};
			_target.Handle(message);

		}
	}
}
