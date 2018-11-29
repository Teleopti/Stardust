using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ExternalPerformancePersisterTest : IIsolateSystem
	{
		public FakeExternalPerformanceRepository ExternalPerformanceRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public IExternalPerformancePersister Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			isolate.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
			isolate.UseTestDouble<ExternalPerformancePersister>().For<IExternalPerformancePersister>();
		}


		[Test]
		public void ShouldPersistExternalPerformanceData()
		{
			const string perfName = "xxx";
			const int perfExtId = 1;
			const ExternalPerformanceDataType numeric = ExternalPerformanceDataType.Numeric;
			var performance = new ExternalPerformance
			{
				ExternalId = perfExtId,
				Name = perfName,
				DataType = numeric
			};

			var extractionInfo = new PerformanceInfoExtractionResult {
				AgentId = "1",
				DateFrom = DateOnly.Today,
				MeasureId = perfExtId,
				MeasureName = perfName,
				MeasureNumberScore = 100.02,
				MeasureType = numeric,
				PersonId = Guid.NewGuid()
			};

			var result = new ExternalPerformanceInfoProcessResult();
			result.ExternalPerformances.Add(performance);
			result.ValidRecords.Add(extractionInfo);

			Target.Persist(result);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count().Should().Be.EqualTo(1);

			var performanceType = ExternalPerformanceRepository.LoadAll();
			performanceType.Count().Should().Be.EqualTo(1);
			performanceType.First().Name.Should().Be.EqualTo(perfName);
			performanceType.First().ExternalId.Should().Be.EqualTo(perfExtId);
			performanceType.First().DataType.Should().Be.EqualTo(numeric);
		}

		[Test]
		public void ShouldOnlyPersistPerfrmanceWhenThereIsNoExternalPerformanceData()
		{
			const string perfName = "xxx";
			const int perfExtId = 1;
			const ExternalPerformanceDataType numeric = ExternalPerformanceDataType.Numeric;
			var performance = new ExternalPerformance
			{
				ExternalId = perfExtId,
				Name = perfName,
				DataType = numeric
			};

			var result = new ExternalPerformanceInfoProcessResult();
			result.ExternalPerformances.Add(performance);

			Target.Persist(result);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count().Should().Be.EqualTo(0);

			var performanceType = ExternalPerformanceRepository.LoadAll();
			performanceType.Count().Should().Be.EqualTo(1);
			performanceType.First().Name.Should().Be.EqualTo(perfName);
			performanceType.First().ExternalId.Should().Be.EqualTo(perfExtId);
			performanceType.First().DataType.Should().Be.EqualTo(numeric);
		}

		[Test]
		public void ShouldNotPersistWhenThereIsNoExternalPerformance()
		{
			const string perfName = "xxx";
			const int perfExtId = 1;
			const ExternalPerformanceDataType numeric = ExternalPerformanceDataType.Numeric;

			var extractionInfo = new PerformanceInfoExtractionResult
			{
				AgentId = "1",
				DateFrom = DateOnly.Today,
				MeasureId = perfExtId,
				MeasureName = perfName,
				MeasureNumberScore = 100.02,
				MeasureType = numeric,
				PersonId = Guid.NewGuid()
			};

			var result = new ExternalPerformanceInfoProcessResult();
			result.ValidRecords.Add(extractionInfo);

			Target.Persist(result);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count().Should().Be.EqualTo(0);

			var performanceType = ExternalPerformanceRepository.LoadAll();
			performanceType.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldUpdateExistingExternalPerformanceData()
		{
			const string perfName = "xxx";
			const int perfExtId = 1;
			var personId = Guid.NewGuid();
			var performance = new ExternalPerformance
			{
				ExternalId = perfExtId,
				Name = perfName,
				DataType = ExternalPerformanceDataType.Percent
			};

			var extractionInfo = new PerformanceInfoExtractionResult
			{
				AgentId = "1",
				DateFrom = DateOnly.Today,
				MeasureId = perfExtId,
				MeasureName = perfName,
				MeasurePercentScore = new Percent(0.8735),
				MeasureType = ExternalPerformanceDataType.Percent,
				PersonId = personId
			};

			var result = new ExternalPerformanceInfoProcessResult();
			result.ExternalPerformances.Add(performance);
			result.ValidRecords.Add(extractionInfo);

			ExternalPerformanceDataRepository.Add(new ExternalPerformanceData
			{
				PersonId = personId,
				DateFrom = extractionInfo.DateFrom,
				OriginalPersonId = extractionInfo.AgentId,
				Score = 0.9123,
				ExternalPerformance = performance
			});

			Target.Persist(result);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count().Should().Be.EqualTo(1);
			performanceData.First().Score.Should().Be.EqualTo(0.8735);

		}

		[Test]
		public void ShouldPersistMultipleValidRecordsOfDifferentTypesOfMeasuresForOneAgent()
		{
			var personId = Guid.NewGuid();
			var perfName = "Just a Measure";
			var perfExtId1 = 1;
			var perfExtId2 = 2;
			var numeric = ExternalPerformanceDataType.Numeric;

			var performanceType1 = new ExternalPerformance
			{
				Name = perfName,
				ExternalId = perfExtId1,
				DataType = numeric
			};

			var performanceType2 = new ExternalPerformance
			{
				Name = perfName,
				ExternalId = perfExtId2,
				DataType = numeric
			};

			var record1 = new PerformanceInfoExtractionResult
			{
				AgentId = "Whatever",
				DateFrom = DateOnly.Today,
				MeasureId = perfExtId1,
				MeasureName = perfName,
				MeasureNumberScore = 100,
				MeasureType = numeric,
				PersonId = personId
			};

			var record2 = new PerformanceInfoExtractionResult
			{
				AgentId = "Whatever",
				DateFrom = DateOnly.Today,
				MeasureId = perfExtId2,
				MeasureName = perfName,
				MeasureNumberScore = 1,
				MeasureType = numeric,
				PersonId = personId
			};

			var data = new ExternalPerformanceInfoProcessResult();
			data.ExternalPerformances.Add(performanceType1);
			data.ExternalPerformances.Add(performanceType2);
			data.ValidRecords.Add(record1);
			data.ValidRecords.Add(record2);

			Target.Persist(data);

			var result = ExternalPerformanceDataRepository.LoadAll().ToList();
			result.Count.Should().Be.EqualTo(2);
			result[0].Score.Should().Be.EqualTo(record1.MeasureNumberScore);
			result[1].Score.Should().Be.EqualTo(record2.MeasureNumberScore);
		}
	}
}
