using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	public class ExternalPerformancePersisterTest : ISetup
	{
		public FakeExternalPerformanceRepository ExternalPerformanceRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public IExternalPerformancePersister Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			system.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
			system.UseTestDouble<ExternalPerformancePersister>().For<IExternalPerformancePersister>();
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
				DateFrom = DateTime.UtcNow,
				GameId = perfExtId,
				GameName = perfName,
				GameNumberScore = 100,
				GameType = numeric,
				PersonId = Guid.NewGuid()
			};

			var result = new ExternalPerformanceInfoProcessResult();
			result.ExternalPerformances.Add(performance);
			result.ValidRecords.Add(extractionInfo);

			Target.Persist(result);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count.Should().Be.EqualTo(1);

			var performanceType = ExternalPerformanceRepository.LoadAll();
			performanceType.Count.Should().Be.EqualTo(1);
			performanceType.First().Name.Should().Be.EqualTo(perfName);
			performanceType.First().ExternalId.Should().Be.EqualTo(perfExtId);
			performanceType.First().DataType.Should().Be.EqualTo(numeric);
		}

		[Test]
		public void ShouldUpdateExistingExternalPerformanceData()
		{
			const string perfName = "xxx";
			const int perfExtId = 1;
			const ExternalPerformanceDataType numeric = ExternalPerformanceDataType.Numeric;
			var personId = Guid.NewGuid();
			var performance = new ExternalPerformance
			{
				ExternalId = perfExtId,
				Name = perfName,
				DataType = numeric
			};

			var extractionInfo = new PerformanceInfoExtractionResult
			{
				AgentId = "1",
				DateFrom = DateTime.UtcNow,
				GameId = perfExtId,
				GameName = perfName,
				GameNumberScore = 100,
				GameType = numeric,
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
				Score = 80
			});


			Target.Persist(result);

			ExternalPerformanceRepository.LoadAll().Count.Should().Be.EqualTo(1);

			var performanceData = ExternalPerformanceDataRepository.LoadAll();
			performanceData.Count.Should().Be.EqualTo(1);
			performanceData.First().Score.Should().Be.EqualTo(100);

		}
	}
}
