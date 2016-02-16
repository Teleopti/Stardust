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

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ImportForecastsToSkillTest
	{
		private ImportForecastsToSkillBase _target;
		private MockRepository _mocks;
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
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
			_skillRepository = _mocks.StrictMock<ISkillRepository>();
			_jobResultRepository = _mocks.StrictMock<IJobResultRepository>();
			_feedback = _mocks.DynamicMock<IJobResultFeedback>();
			_messageBroker = _mocks.DynamicMock<IMessageBrokerComposite>();
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_jobResult = _mocks.DynamicMock<IJobResult>();
			_disableFilter = _mocks.DynamicMock<IDisableBusinessUnitFilter>();
			_saveForecastToSkillCommand = _mocks.StrictMock<ISaveForecastToSkillCommand>();
			_target = new ImportForecastsToSkillBase(_unitOfWorkFactory, _saveForecastToSkillCommand,
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
			using (_mocks.Record())
			{
				var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(_unitOfWorkFactory.Current()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
				Expect.Call(_skillRepository.Get(skillId)).Return(skill);
				Expect.Call(() =>
					 _saveForecastToSkillCommand.Execute(dateTime, skill, new[] { row }, ImportForecastsMode.ImportWorkload));
			}
			using (_mocks.Playback())
			{
				var message = new ImportForecastsToSkill
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
}
