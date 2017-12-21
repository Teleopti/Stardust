using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ImportExternalPerformanceInfoHandlerTest : ISetup
	{
		public ImportExternalPerformanceInfoHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakeExternalPerformanceRepository ExternalPerformanceTypeRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public FakePersonRepository PersonRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeDatabase>();
			system.AddService<FakeStorage>();
			system.UseTestDouble<ImportExternalPerformanceInfoHandler>().For<IHandleEvent<ImportExternalPerformanceInfoEvent>>();
			system.UseTestDouble<ExternalPerformanceInfoFileProcessor>().For<IExternalPerformanceInfoFileProcessor>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			system.UseTestDouble<ImportJobArtifactValidator>().For<IImportJobArtifactValidator>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<FakeTenantLogonDataManager>().For<ITenantLogonDataManager>();
			system.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			system.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
		}

		[Test]
		public void ShouldResolveTarget()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSaveNewExternalPerformanceType()
		{
			var expectedId = 8;
			var expectedName = "Quality Score";
			var expectedDataType = ExternalPerformanceDataType.Percent;
			var processResult = new ExternalPerformanceInfoProcessResult()
			{
				ExternalPerformances = new List<IExternalPerformance>()
			{
				new ExternalPerformance(){ExternalId = expectedId, Name = expectedName, DataType = expectedDataType}
			}
			};

			Target.Handle(NewEvent());

			var result = ExternalPerformanceTypeRepository.FindAllExternalPerformances();
			result.Count().Should().Be.EqualTo(1);
			result.ToList()[0].ExternalId.Should().Be.EqualTo(expectedId);
			result.ToList()[0].Name.Should().Be.EqualTo(expectedName);
			result.ToList()[0].DataType.Should().Be.EqualTo(expectedDataType);
		}

		[Test]
		public void ShouldSaveValidRecord()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle").WithId();
			const string employmentNum = "1";
			person.SetEmploymentNumber(employmentNum);
			PersonRepository.Add(person);

			var expectedDate = new DateTime(2017, 11, 20);
			var expectedScore = 87;
			var expectedPersonId = Guid.NewGuid();
			var gameId = 8;
			var expectedExternalPerformance = new ExternalPerformance() { ExternalId = gameId, DataType = ExternalPerformanceDataType.Percent };
			expectedExternalPerformance.WithId(Guid.NewGuid());
			ExternalPerformanceTypeRepository.Add(expectedExternalPerformance);

			Target.Handle(NewEvent());


			var result = ExternalPerformanceDataRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].OriginalPersonId.Should().Be.EqualTo(employmentNum);
			result.ToList()[0].Score.Should().Be.EqualTo(expectedScore);
			result.ToList()[0].DateFrom.Should().Be.EqualTo(expectedDate);
			result.ToList()[0].PersonId.Should().Be.EqualTo(person.Id.Value);
			result.ToList()[0].ExternalPerformance.ExternalId.Should().Be.EqualTo(expectedExternalPerformance.ExternalId);
		}

		[Test]
		public void ShouldUpdateExistingDataWithValidRecord()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle").WithId();
			const string employmentNum = "1";
			person.SetEmploymentNumber(employmentNum);
			PersonRepository.Add(person);

			var originalAgentId = "1";
			var date = new DateTime(2017, 11, 20);
			var oldScore = 100;
			var newScore = 87;
			var personId = person.Id.Value;
			var gameId = 8;

			var extPerfType = new ExternalPerformance { ExternalId = gameId, DataType = ExternalPerformanceDataType.Percent };
			extPerfType.WithId(Guid.NewGuid());

			ExternalPerformanceTypeRepository.Add(extPerfType);

			ExternalPerformanceDataRepository.Add(new ExternalPerformanceData
			{
				DateFrom = date,
				ExternalPerformance = extPerfType,
				PersonId = personId,
				OriginalPersonId = originalAgentId,
				Score = oldScore
			});

			Target.Handle(NewEvent());

			var result = ExternalPerformanceDataRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].PersonId.Should().Be.EqualTo(personId);
			result.ToList()[0].Score.Should().Be.EqualTo(newScore);
		}

		private ImportExternalPerformanceInfoEvent NewEvent()
		{
			return new ImportExternalPerformanceInfoEvent()
			{
				JobResultId = NewJobResult().Id.Value
			};
		}

		private IJobResult NewJobResult()
		{
			var currentUser = new FakeLoggedOnUser();
			var importFileData = new ImportFileData() { FileName = "test.csv" };

			importFileData.Data = Encoding.UTF8.GetBytes("20171120,1,Kalle,Pettersson,Quality Score,8,Percent,0.87");

			var jobResultArtifact = new JobResultArtifact(JobResultArtifactCategory.Input, importFileData.FileName, importFileData.Data);
			var jobResult = new JobResult(JobCategory.WebImportExternalGamification, DateOnly.Today.ToDateOnlyPeriod(), currentUser.CurrentUser(), DateTime.UtcNow).WithId();

			jobResult.AddArtifact(jobResultArtifact);
			jobResult.SetVersion(1);
			JobResultRepository.Add(jobResult);

			return JobResultRepository.Get(jobResult.Id.Value);
		}
	}
}
