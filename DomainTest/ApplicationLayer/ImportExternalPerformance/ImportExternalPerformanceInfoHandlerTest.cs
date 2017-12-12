using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture]
	public class ImportExternalPerformanceInfoHandlerTest
	{
		private FakeJobResultRepository _jobResultRepository;
		private FakeStardustJobFeedback _stardustJobFeedback;
		private IImportJobArtifactValidator _importJobArtifactValidator;
		private IExternalPerformanceInfoFileProcessor _externalPerformanceInfoFileProcessor;
		private FakeExternalPerformanceRepository _externalPerformanceRepository;
		private FakeExternalPerformanceDataRepository _externalPerformanceDataRepository;

		private ImportFileData _importFileData;
		private JobResultArtifact _jobResultArtifact;
		private JobResult _jobResult;
		private Guid _jobResultId;

		[SetUp]
		public void Setup()
		{
			var currentUser = new FakeLoggedOnUser();
			_importFileData = new ImportFileData() { FileName = "test.csv" };
			_jobResultArtifact = new JobResultArtifact(JobResultArtifactCategory.Input, _importFileData.FileName, _importFileData.Data);
			_jobResult = new JobResult(JobCategory.WebImportExternalGamification, DateOnly.Today.ToDateOnlyPeriod(), currentUser.CurrentUser(), DateTime.UtcNow);
			_jobResultId = Guid.NewGuid();
			_jobResult.WithId(_jobResultId);

			_jobResultRepository = new FakeJobResultRepository();
			_stardustJobFeedback = new FakeStardustJobFeedback();
			_importJobArtifactValidator = MockRepository.GenerateMock<IImportJobArtifactValidator>();
			_importJobArtifactValidator.Stub(x=>x.ValidateJobArtifact(_jobResult, _stardustJobFeedback.SendProgress)).Return(_jobResultArtifact);
			_externalPerformanceInfoFileProcessor = MockRepository.GenerateMock<IExternalPerformanceInfoFileProcessor>();
			_externalPerformanceRepository = new FakeExternalPerformanceRepository();
			_externalPerformanceDataRepository = new FakeExternalPerformanceDataRepository();
		}

		[Test]
		public void ShouldSaveSettingData()
		{
			var expectedId = 1;
			var expectedName = "externalSetting";
			var expectedDataType = ExternalPerformanceDataType.Percentage;
			var processResult = new ExternalPerformanceInfoProcessResult(){ExternalPerformances = new List<IExternalPerformance>()
			{
				new ExternalPerformance(){ExternalId = expectedId, Name = expectedName, DataType = expectedDataType}
			}};
			_externalPerformanceInfoFileProcessor.Stub(x => x.Process(null, null)).IgnoreArguments().Return(processResult);
			var target = new ImportExternalPerformanceInfoHandler(_jobResultRepository, _stardustJobFeedback, _importJobArtifactValidator, 
				_externalPerformanceInfoFileProcessor,_externalPerformanceRepository, _externalPerformanceDataRepository);

			target.Handle(getEvent());

			var result = _externalPerformanceRepository.FindAllExternalPerformances();
			result.Count().Should().Be.EqualTo(1);
			result.ToList()[0].ExternalId.Should().Be.EqualTo(expectedId);
			result.ToList()[0].Name.Should().Be.EqualTo(expectedName);
			result.ToList()[0].DataType.Should().Be.EqualTo(expectedDataType);
		}

		[Test]
		public void ShouldSaveValidRecord()
		{
			var expectedAgentId = "Ashley";
			var expectedDate = DateOnly.Today;
			var expectedScore = 200;
			var expectedPersonId = Guid.NewGuid();
			var gameId = 1;
			var expectedExternalPerformanceId = Guid.NewGuid();
			var externalPerformance = new ExternalPerformance() {ExternalId = gameId};
			externalPerformance.WithId(expectedExternalPerformanceId);
			_externalPerformanceRepository.Add(externalPerformance);
			var processResult = new ExternalPerformanceInfoProcessResult(){ValidRecords = new List<PerformanceInfoExtractionResult>()
			{
				new PerformanceInfoExtractionResult()
				{
					AgentId = expectedAgentId, DateFrom = expectedDate, PersonId = expectedPersonId,GameId = gameId, GameNumberScore = expectedScore, GameType = "numuric"
				}
			}};
			_externalPerformanceInfoFileProcessor.Stub(x => x.Process(null, null)).IgnoreArguments().Return(processResult);

			var target = new ImportExternalPerformanceInfoHandler(_jobResultRepository, _stardustJobFeedback, _importJobArtifactValidator, 
				_externalPerformanceInfoFileProcessor,_externalPerformanceRepository, _externalPerformanceDataRepository);

			target.Handle(getEvent());

			var result = _externalPerformanceDataRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].OriginalPersonId.Should().Be.EqualTo(expectedAgentId);
			result.ToList()[0].Score.Should().Be.EqualTo(expectedScore);
			result.ToList()[0].DateFrom.Should().Be.EqualTo(expectedDate);
			result.ToList()[0].Person.Should().Be.EqualTo(expectedPersonId);
			result.ToList()[0].ExternalPerformance.Should().Be.EqualTo(expectedExternalPerformanceId);
		}

		[Test]
		public void ShouldUpdateValidRecord()
		{
			var existOriginalAgentId = "Ashley";
			var existDate = DateOnly.Today;
			var expectedScore = 200;
			var existPersonId = Guid.NewGuid();
			var gameId = 1;
			var existExternalPerformanceId = Guid.NewGuid();
			var existExternalPerformance = new ExternalPerformance() { ExternalId = gameId };
			existExternalPerformance.WithId(existExternalPerformanceId);
			_externalPerformanceRepository.Add(existExternalPerformance);
			var processResult = new ExternalPerformanceInfoProcessResult
			{
				ValidRecords = new List<PerformanceInfoExtractionResult>
				{
					new PerformanceInfoExtractionResult
					{
						AgentId = existOriginalAgentId, DateFrom = existDate, PersonId = existPersonId,GameId = gameId,
						GameNumberScore = expectedScore, GameType = "numuric"
					}
				}
			};
			_externalPerformanceInfoFileProcessor.Stub(x => x.Process(null, null)).IgnoreArguments().Return(processResult);
			_externalPerformanceDataRepository.Add(new ExternalPerformanceData
			{
				DateFrom = existDate, ExternalPerformance = existExternalPerformanceId,
				Person = existPersonId,
				OriginalPersonId = existOriginalAgentId,Score = 100
			});

			var target = new ImportExternalPerformanceInfoHandler(_jobResultRepository, _stardustJobFeedback, _importJobArtifactValidator,
				_externalPerformanceInfoFileProcessor, _externalPerformanceRepository, _externalPerformanceDataRepository);

			target.Handle(getEvent());

			var result = _externalPerformanceDataRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].Person.Should().Be.EqualTo(existPersonId);
			result.ToList()[0].Score.Should().Be.EqualTo(expectedScore);
		}

		private ImportExternalPerformanceInfoEvent getEvent()
		{
			_jobResult.AddArtifact(_jobResultArtifact);
			_jobResultRepository.Add(_jobResult);

			return new ImportExternalPerformanceInfoEvent()
			{
				JobResultId = _jobResultId
			};
		}
	}
}
