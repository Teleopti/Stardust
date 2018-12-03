using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture, DomainTest]
	public class ImportAgentJobServiceTest : IIsolateSystem
	{
		public IJobResultRepository JobResultRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeEventPublisher Publisher;
		public IImportAgentJobService Target;
		public ImportAgentEventHandler EventHandler;
		public ICurrentTenantUser CurrentTenantUser;
		public CurrentBusinessUnit CurrentBusinessUnit;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ImportAgentJobService>().For<IImportAgentJobService>();
			isolate.UseTestDouble<FakeJobResultRepositoryForCurrentBusinessUnit>().For<IJobResultRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble<ImportAgentEventHandler>().For<IHandleEvent<ImportAgentEvent>>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		[Test]
		public void ShouldResolveTheTarget()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateJob()
		{
			setCurrentLoggedOnUser();
			CurrentBusinessUnit.OnThisThreadUse(BusinessUnitFactory.CreateWithId("bu"));
			var job = createValidJob();
			var result = JobResultRepository.LoadAll().Single();

			result.JobCategory.Should().Be(JobCategory.WebImportAgent);
			result.Owner.Id.Should().Be(LoggedOnUser.CurrentUser().Id);
			result.Artifacts.Single().Category.Should().Be(JobResultArtifactCategory.Input);
		}

		[Test]
		public void ShouldPublishImportAgentEvent()
		{
			setCurrentLoggedOnUser();
			var fallbacks = new ImportAgentDefaults();
			Publisher.Clear();
			var job = createValidJob(new ImportAgentDefaults());

			var @event = Publisher.PublishedEvents.Single() as ImportAgentEvent;
			@event.Should().Not.Be.Null();
			@event.JobResultId.Should().Be(job.Id);
			@event.Defaults.Should().Equals(fallbacks);
		}

		[Test]
		public void ShouldGetJobsForCurrentBusinessUnit()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu1");
			var person1 = setCurrentLoggedOnUser();
			CurrentBusinessUnit.OnThisThreadUse(bu1);
			var result = createValidJob() as JobResult;
			result.BusinessUnit.Should().Be(bu1);

			var bu2 = BusinessUnitFactory.CreateWithId("bu2");
			CurrentBusinessUnit.OnThisThreadUse(bu2);
			var person2 = setCurrentLoggedOnUser();
			createValidJob();
			var person3 = setCurrentLoggedOnUser();
			createValidJob();
			

			var list = Target.GetJobsForCurrentBusinessUnit();
			list.Count.Should().Be(2);
			list.Any(j => j.JobResult.Owner.Id == person1.Id).Should().Be.False();
			list.Any(j => j.JobResult.Owner.Id == person2.Id).Should().Be.True();
			list.Any(j => j.JobResult.Owner.Id == person3.Id).Should().Be.True();
		}

		[Test]
		public void ShouldGetCorrectJobsDetailWhenJobIsNotFinished()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu1");
			CurrentBusinessUnit.OnThisThreadUse(bu1);
			setCurrentLoggedOnUser();
			createValidJob();
			var detail = Target.GetJobsForCurrentBusinessUnit().FirstOrDefault();

			detail.JobResult.IsWorking().Should().Be(true);
		}


		[Test]
		public void ShouldGetCorrectJobsDetailWhenJobIsFinished()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu1");
			CurrentBusinessUnit.OnThisThreadUse(bu1);
			var job1 = createValidJob();

			Assert.Throws<ZipException>(() => EventHandler.Handle(new ImportAgentEvent
			{
				JobResultId = job1.Id.GetValueOrDefault()
			}));
			var detail = Target.GetJobsForCurrentBusinessUnit().FirstOrDefault();
			detail.JobResult.IsWorking().Should().Be(false);
			detail.ResultDetail?.DetailLevel.Should().Be(DetailLevel.Error);
			detail.ResultDetail?.ExceptionMessage.Should().Not.Be.Empty();

		}

		[Test]
		public void ShouldGetArtifactIfItExists()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu1");
			CurrentBusinessUnit.OnThisThreadUse(bu1);
			var job1 = createValidJob();
			job1.Artifacts.Add(new JobResultArtifact(JobResultArtifactCategory.OutputError, "test_error.xlsx", Encoding.ASCII.GetBytes("test")));

			var artifact = Target.GetJobResultArtifact(job1.Id.GetValueOrDefault(), JobResultArtifactCategory.Input);
			artifact.Name.Should().Be("test.xlsx");
			artifact.Content.Should().Equals(Encoding.ASCII.GetBytes("test"));

			artifact = Target.GetJobResultArtifact(job1.Id.GetValueOrDefault(), JobResultArtifactCategory.OutputError);
			artifact.Name.Should().Be("test_error.xlsx");
			artifact.Content.Should().Equals(Encoding.ASCII.GetBytes("test"));

			artifact = Target.GetJobResultArtifact(job1.Id.GetValueOrDefault(), JobResultArtifactCategory.OutputWarning);
			artifact.Should().Be.Null();
		}



		private IPerson setCurrentLoggedOnUser()
		{
			var person = PersonFactory.CreatePersonWithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			return person;
		}



		private IJobResult createValidJob(ImportAgentDefaults fallbacks = null)
		{
			var fileData = new ImportFileData()
			{
				FileName = "test.xlsx",
				Data = Encoding.ASCII.GetBytes("test")
			};
			return Target.CreateJob(fileData, fallbacks);
		}
	}
}
