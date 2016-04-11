using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class ImportForecastsFileToSkillTest
	{
		private ImportForecastsFileToSkillBase _target;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private ISkillRepository _skillRepository;
		private IJobResultRepository _jobResultRepository;
		private IImportForecastsRepository _importForecastsRepository;
		private IJobResultFeedback _feedback;
		private IMessageBrokerComposite _messageBroker;
		private IOpenAndSplitTargetSkillHandler _openAndSplitTargetSkillHandler;
		private IForecastsFileContentProvider _contentProvider;
		private TimeZoneInfo _timeZone;
		private IUnitOfWork _unitOfWork;
		private IJobResult _jobResult;
		private IForecastFile _forecastFile;
		private IForecastsAnalyzeQuery _analyzeQuery;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_jobResultRepository = MockRepository.GenerateMock<IJobResultRepository>();
			_importForecastsRepository = MockRepository.GenerateMock<IImportForecastsRepository>();
			_contentProvider = MockRepository.GenerateMock<IForecastsFileContentProvider>();
			_feedback = MockRepository.GenerateMock<IJobResultFeedback>();
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_openAndSplitTargetSkillHandler = MockRepository.GenerateMock<IOpenAndSplitTargetSkillHandler>();
			_timeZone = (TimeZoneInfo.Utc);
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_jobResult = MockRepository.GenerateMock<IJobResult>();
			_forecastFile = MockRepository.GenerateMock<IForecastFile>();
			_analyzeQuery = MockRepository.GenerateMock<IForecastsAnalyzeQuery>();
			_target = new ImportForecastsFileToSkillBase(_unitOfWorkFactory, _skillRepository, _jobResultRepository,
																			 _importForecastsRepository, _contentProvider, _analyzeQuery, _feedback,
																			 _messageBroker, _openAndSplitTargetSkillHandler);
		}

		[Test]
		public void ShouldHandleMessageCorrectly()
		{
			var fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;

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

			var queryResult = new ForecastsAnalyzeQueryResult
			{
				Succeeded = true,
				Period = new DateOnlyPeriod(dateTime, dateTime),
				WorkloadDayOpenHours = openHours,
				ForecastFileContainer = forecasts
			};

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill).Repeat.Any();
			_importForecastsRepository.Stub(x => x.Get(jobId)).Return(_forecastFile);
			_forecastFile.Stub(x => x.FileContent).Return(fileContent);
			_contentProvider.Stub(x => x.LoadContent(fileContent, _timeZone)).Return(new[] { row });
			_analyzeQuery.Stub(x => x.Run(new[] { row }, skill)).Return(queryResult);
			_feedback.Stub(x => x.SetJobResult(_jobResult, _messageBroker));
			_openAndSplitTargetSkillHandler.Stub(x => x.Handle(null)).IgnoreArguments();

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

		[Test]
		public void ShouldSendValidationErrorMessageWhenNoSkillFound()
		{
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_skillRepository.Stub(x => x.Get(skillId)).Return(null);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);

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

		[Test]
		public void ShouldSendValidationErrorMessageWhenNoFileFound()
		{
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_importForecastsRepository.Stub(x => x.Get(jobId)).Return(null);

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

		[Test]
		public void ShouldSendValidationErrorMessageWhenFileAnalysisFailed()
		{
			var fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179");
			var jobId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("test skill");
			skill.TimeZone = _timeZone;
			var queryResult = MockRepository.GenerateMock<IForecastsAnalyzeQueryResult>();

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			_jobResultRepository.Stub(x => x.Get(jobId)).Return(_jobResult);
			_importForecastsRepository.Stub(x => x.Get(jobId)).Return(_forecastFile);
			_forecastFile.Stub(x => x.FileContent).Return(fileContent);
			_contentProvider.Stub(x => x.LoadContent(fileContent, _timeZone)).Return(new List<IForecastsRow>());
			_analyzeQuery.Stub(x => x.Run(new List<IForecastsRow>(), skill)).Return(queryResult);
			queryResult.Stub(x => x.Succeeded).Return(false).Repeat.Any();
			queryResult.Stub(x => x.ErrorMessage).Return("error occured.");

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
