using System;
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
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ImportExternalPerformanceInfoHandlerTest : IIsolateSystem
	{
		public ImportExternalPerformanceInfoHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakeExternalPerformanceRepository ExternalPerformanceTypeRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ImportExternalPerformanceInfoHandler>().For<IHandleEvent<ImportExternalPerformanceInfoEvent>>();
			isolate.UseTestDouble<ExternalPerformanceInfoFileProcessor>().For<IExternalPerformanceInfoFileProcessor>();
			isolate.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			isolate.UseTestDouble<ImportJobArtifactValidator>().For<IImportJobArtifactValidator>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<FakeTenantLogonDataManager>().For<ITenantLogonDataManager>();
			isolate.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			isolate.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
			isolate.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
		}

		[Test]
		public void ShouldResolveHandler()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHandleEventWithNoAtifact()
		{
			var updated = NewJobResult();

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = updated.Id.Value
			});

			updated.Details.Count().Should().Be(1);
			updated.FinishedOk.Should().Be.True();
		}

		[Test]
		public void ShouldHandleEventWhenThereAreMultipleInputArtifacts()
		{
			var updated = NewJobResult();
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.csv", null));
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test1.csv", null));

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = updated.Id.Value
			});

			updated.Details.Count().Should().Be(1);
			updated.FinishedOk.Should().Be.True();
			updated.Details.Single().Message.Should().Be(Resources.InvalidInput);
		}

		[Test]
		public void ShouldHandleEventWithEmptyInputArtifact()
		{
			var jobResult = NewJobResult();

			var fileContent = Encoding.UTF8.GetBytes("");
			var artifact = new JobResultArtifact(JobResultArtifactCategory.Input, "test.csv", fileContent);
			jobResult.AddArtifact(artifact);

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Details.Single().DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldHandleEventWhenRecordsAreValid()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle").WithId();
			const string employmentNum = "1";
			person.SetEmploymentNumber(employmentNum);
			PersonFinderReadOnlyRepository.Has(person);

			var jobResult = NewJobResult();
			string stringContent = "20171120,1,Kalle,Pettersson,Quality Score,1,Percent,87\n" + "20171120,1,Kalle,Pettersson,Sales Result,2,Numeric,2000";
			var fileContent = Encoding.UTF8.GetBytes(stringContent);
			var artifact = new JobResultArtifact(JobResultArtifactCategory.Input, "test.csv", fileContent);
			jobResult.AddArtifact(artifact);

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Artifacts.Count.Should().Be.EqualTo(1);
			jobResult.Artifacts[0].Name.Should().Be.EqualTo("test.csv");
		}

		[Test]
		public void ShouldNotHandleEventWhenThereAreInvalidRecords()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle").WithId();
			const string employmentNum = "1";
			person.SetEmploymentNumber(employmentNum);
			PersonFinderReadOnlyRepository.Has(person);

			var jobResult = NewJobResult();
			string stringContent = "20171120,1,Kalle,Pettersson,Quality Score,1,Percent,87\n" + "20171120,137727,Kalle,Pettersson,Sales Result,2,aa,2000";
			var fileContent = Encoding.UTF8.GetBytes(stringContent);
			var artifact = new JobResultArtifact(JobResultArtifactCategory.Input, "test.csv", fileContent);
			jobResult.AddArtifact(artifact);

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Artifacts.Count.Should().Be.EqualTo(2);
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.Input).Should().Be(1);
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputError).Should().Be(1);
		}

		[Test]
		public void ShouldHandleJobOnlyOnce()
		{
			var jobResult = NewJobResult();

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Version.GetValueOrDefault().Should().Be(2);
			jobResult.Details.Count().Should().Be(1);
		}

		[Test]
		public void ShouldJobDetailLevelBeInfoIfJobExecuteSucceed()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle").WithId();
			const string employmentNum = "1";
			person.SetEmploymentNumber(employmentNum);
			PersonFinderReadOnlyRepository.Has(person);

			var jobResult = NewJobResult();
			var fileContent = Encoding.UTF8.GetBytes("20171120,1,Kalle,Pettersson,Quality Score,1,Percent,87");
			var artifact = new JobResultArtifact(JobResultArtifactCategory.Input, "test.csv", fileContent);
			jobResult.AddArtifact(artifact);

			Target.Handle(new ImportExternalPerformanceInfoEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Details.Single().DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
		}

		private IJobResult NewJobResult()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult(JobCategory.WebImportExternalGamification, DateOnly.Today.ToDateOnlyPeriod(), person, DateTime.UtcNow).WithId();
			jobResult.SetVersion(1);
			JobResultRepository.Add(jobResult);
			var updated = JobResultRepository.Get(jobResult.Id.Value);
			return updated;
		}
	}
}
