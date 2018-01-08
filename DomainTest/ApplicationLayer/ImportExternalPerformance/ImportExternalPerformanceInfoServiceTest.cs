using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ImportExternalPerformanceInfoServiceTest: ISetup
	{
		public CurrentBusinessUnit CurrentBusinessUnit;
		public IJobResultRepository JobResultRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public IImportExternalPerformanceInfoService Target;
		public ICurrentTenantUser CurrentTenantUser;
		public FakeEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ImportExternalPerformanceInfoService>().For<IImportExternalPerformanceInfoService>();
			system.UseTestDouble<FakeJobResultRepositoryForCurrentBusinessUnit>().For<IJobResultRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			system.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		[Test]
		public void ShouldCreateJob()
		{
			setCurrentLoggedOnUser();
			CurrentBusinessUnit.OnThisThreadUse(BusinessUnitFactory.CreateWithId("bu"));
			createValidJob();
			var result = JobResultRepository.LoadAll().Single();

			result.JobCategory.Should().Be(JobCategory.WebImportExternalGamification);
			result.Owner.Id.Should().Be(LoggedOnUser.CurrentUser().Id);
			result.Artifacts.Single().Category.Should().Be(JobResultArtifactCategory.Input);
		}

		[Test]
		public void ShouldPublishImportAgentEvent()
		{
			setCurrentLoggedOnUser();
			var job = createValidJob();

			var @event = Publisher.PublishedEvents.Single() as ImportExternalPerformanceInfoEvent;
			@event.Should().Not.Be.Null();
			@event.JobResultId.Should().Be(job.Id);
		}

		[Test]
		public void ShouldGetImportedJobs()
		{
			setCurrentLoggedOnUser();
			var job = createValidJob();

			var result = Target.GetJobsForCurrentBusinessUnit();
			result.Count.Should().Be(1);
		}

		private IPerson setCurrentLoggedOnUser()
		{
			var person = PersonFactory.CreatePersonWithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			return person;
		}

		private IJobResult createValidJob()
		{
			var fileData = new ImportFileData
			{
				FileName = "test.csv",
				Data = Encoding.ASCII.GetBytes("test")
			};
			return Target.CreateJob(fileData);
		}
	}
}
