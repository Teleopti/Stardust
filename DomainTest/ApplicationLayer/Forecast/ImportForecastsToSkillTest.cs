using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class ImportForecastsToSkillTest
	{
		private ImportForecastProcessor _target;
		private ICurrentUnitOfWork _unitOfWorkFactory;
		private ISkillRepository _skillRepository;
		private IJobResultRepository _jobResultRepository;
		private IJobResultFeedback _feedback;
		private IMessageBrokerComposite _messageBroker;
		private ISaveForecastToSkill _saveForecastToSkill;
		private IJobResult _jobResult;
		private IDisableBusinessUnitFilter _disableFilter;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_jobResultRepository = MockRepository.GenerateMock<IJobResultRepository>();
			_feedback = MockRepository.GenerateMock<IJobResultFeedback>();
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_jobResult = MockRepository.GenerateMock<IJobResult>();
			_disableFilter = MockRepository.GenerateMock<IDisableBusinessUnitFilter>();
			_saveForecastToSkill = MockRepository.GenerateMock<ISaveForecastToSkill>();
			_target = new ImportForecastProcessor(_unitOfWorkFactory, _saveForecastToSkill,
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

			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uow);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			_saveForecastToSkill.Stub(x => x.Execute(dateTime, skill, new[] { row }, ImportForecastsMode.ImportWorkload));

			var message = new ImportForecastProcessorMessage
			{
				JobId = jobId,
				ImportMode = ImportForecastsMode.ImportWorkload,
				TargetSkillId = skillId,
				Date = dateTime.Date,
				Forecasts = new[] { row },
				Timestamp = DateTime.Now
			};
			_target.Process(message);

		}
	}
}
