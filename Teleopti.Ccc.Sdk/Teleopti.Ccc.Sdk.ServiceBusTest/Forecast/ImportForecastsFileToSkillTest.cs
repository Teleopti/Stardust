using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ImportForecastsFileToSkillTest
	{
		private ImportForecastsFileToSkillBase _target;
		private MockRepository _mocks;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private ISkillRepository _skillRepository;
		private IJobResultRepository _jobResultRepository;
		private IImportForecastsRepository _importForecastsRepository;
		private IJobResultFeedback _feedback;
		private IMessageBrokerComposite _messageBroker;
		private IEventPublisher _serviceBus;
		private IForecastsFileContentProvider _contentProvider;
		private TimeZoneInfo _timeZone;
		private IUnitOfWork _unitOfWork;
		private IJobResult _jobResult;
		private IForecastFile _forecastFile;
		private IForecastsAnalyzeQuery _analyzeQuery;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
			_skillRepository = _mocks.StrictMock<ISkillRepository>();
			_jobResultRepository = _mocks.StrictMock<IJobResultRepository>();
			_importForecastsRepository = _mocks.StrictMock<IImportForecastsRepository>();
			_contentProvider = _mocks.StrictMock<IForecastsFileContentProvider>();
			_feedback = _mocks.DynamicMock<IJobResultFeedback>();
			_messageBroker = _mocks.StrictMock<IMessageBrokerComposite>();
			_serviceBus = _mocks.StrictMock<IEventPublisher>();
			_timeZone = (TimeZoneInfo.Utc);
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_jobResult = _mocks.DynamicMock<IJobResult>();
			_forecastFile = _mocks.DynamicMock<IForecastFile>();
			_analyzeQuery = _mocks.StrictMock<IForecastsAnalyzeQuery>();
			_target = new ImportForecastsFileToSkillBase(_unitOfWorkFactory, _skillRepository, _jobResultRepository,
																			 _importForecastsRepository, _contentProvider, _analyzeQuery, _feedback,
																			 _messageBroker, _serviceBus);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldHandleMessageCorrectly()
		{
			var fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;
			var queryResult = _mocks.StrictMock<IForecastsAnalyzeQueryResult>();
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
			var dateTime = new DateOnly(2012, 3, 1);
			var openHours = new WorkloadDayOpenHoursContainer();
			openHours.AddOpenHour(dateTime, new TimePeriod(12, 45, 13, 0));
			var forecasts = new ForecastFileContainer();
			forecasts.AddForecastsRow(dateTime, row);
			using (_mocks.Record())
			{
				var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(_unitOfWorkFactory.Current()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
				Expect.Call(_skillRepository.Get(skillId)).Return(skill).Repeat.Any();
				Expect.Call(_importForecastsRepository.Get(jobId)).Return(_forecastFile);
				Expect.Call(_forecastFile.FileContent).Return(fileContent);
				Expect.Call(_contentProvider.LoadContent(fileContent, _timeZone)).Return(new[] { row });
				Expect.Call(_analyzeQuery.Run(new[] { row }, skill)).Return(queryResult);
				Expect.Call(queryResult.Succeeded).Return(true).Repeat.Any();
				Expect.Call(queryResult.Period).Return(new DateOnlyPeriod(dateTime, dateTime)).Repeat.Any();
				Expect.Call(queryResult.WorkloadDayOpenHours).Return(openHours);
				Expect.Call(queryResult.ForecastFileContainer).Return(forecasts);
				Expect.Call(() => _feedback.SetJobResult(_jobResult, _messageBroker));
				Expect.Call(() => _serviceBus.Publish()).Constraints(
					 Is.Matching<Object[]>(a => ((OpenAndSplitTargetSkill)a[0]).Date == dateTime.Date));
			}
			using (_mocks.Playback())
			{
				var message = new ImportForecastsFileToSkill
				{
					JobId = jobId,
					ImportMode = ImportForecastsMode.ImportWorkload,
					TargetSkillId = skillId,
					UploadedFileId = jobId,
					Timestamp = DateTime.Now
				};
				_target.Handle(message);
			}
		}

		[Test]
		public void ShouldSendValidationErrorMessageWhenNoSkillFound()
		{
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			using (_mocks.Record())
			{
				var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(_unitOfWorkFactory.Current()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_skillRepository.Get(skillId)).Return(null);
				Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
			}
			using (_mocks.Playback())
			{
				var message = new ImportForecastsFileToSkill
				{
					JobId = jobId,
					ImportMode = ImportForecastsMode.ImportWorkload,
					TargetSkillId = skillId,
					UploadedFileId = jobId,
					Timestamp = DateTime.Now
				};
				_target.Handle(message);
			}
		}

		[Test]
		public void ShouldSendValidationErrorMessageWhenNoFileFound()
		{
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;
			using (_mocks.Record())
			{
				var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(_unitOfWorkFactory.Current()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_skillRepository.Get(skillId)).Return(skill);
				Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
				Expect.Call(_importForecastsRepository.Get(jobId)).Return(null);
			}
			using (_mocks.Playback())
			{
				var message = new ImportForecastsFileToSkill
				{
					JobId = jobId,
					ImportMode = ImportForecastsMode.ImportWorkload,
					TargetSkillId = skillId,
					UploadedFileId = jobId,
					Timestamp = DateTime.Now
				};
				_target.Handle(message);
			}
		}

		[Test]
		public void ShouldSendValidationErrorMessageWhenFileAnalysisFailed()
		{
			var fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179");
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;
			var queryResult = _mocks.StrictMock<IForecastsAnalyzeQueryResult>();
			using (_mocks.Record())
			{
				var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
				Expect.Call(_unitOfWorkFactory.Current()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_skillRepository.Get(skillId)).Return(skill);
				Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
				Expect.Call(_importForecastsRepository.Get(jobId)).Return(_forecastFile);
				Expect.Call(_forecastFile.FileContent).Return(fileContent);
				Expect.Call(_contentProvider.LoadContent(fileContent, _timeZone)).Return(new List<IForecastsRow>());
				Expect.Call(_analyzeQuery.Run(new List<IForecastsRow>(), skill)).Return(queryResult);
				Expect.Call(queryResult.Succeeded).Return(false).Repeat.Any();
				Expect.Call(queryResult.ErrorMessage).Return("error occured.");
			}
			using (_mocks.Playback())
			{
				var message = new ImportForecastsFileToSkill
				{
					JobId = jobId,
					ImportMode = ImportForecastsMode.ImportWorkload,
					TargetSkillId = skillId,
					UploadedFileId = jobId,
					Timestamp = DateTime.Now
				};
				_target.Handle(message);
			}
		}
	}
}
