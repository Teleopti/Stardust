using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Import
{
	[TestFixture, DomainTest]
	public class ImportAgentServiceTest:ISetup
	{
		public FakeJobResultRepository JobResultRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeEventPublisher Publisher;
		public IImportAgentJobService Target;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ImportAgentJobService>().For<IImportAgentJobService>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();

		}

		[Test]
		public void ShouldResolveTheTarget()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateJob()
		{
			var person = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var fileData = new FileData()
			{
				FileName = "test.xlsx",
				Data = Encoding.ASCII.GetBytes("test")
			};

			Target.CreateJob(fileData, null, person);

			var result = JobResultRepository.LoadAll().Single();

			result.JobCategory.Should().Be(JobCategory.WebImportAgent);
			result.Owner.Id.Should().Be(person.Id);
			result.Artifacts.Single().Category.Should().Be(JobResultArtifactCategory.Input);
		}

		[Test]
		public void ShouldPublishImportAgentEvent()
		{
			var person = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var fileData = new FileData()
			{
				FileName = "test.xlsx",
				Data = Encoding.ASCII.GetBytes("test")
			};

			var fallbacks = new ImportAgentDefaults();

			var job = Target.CreateJob(fileData, fallbacks, person);

			var @event = Publisher.PublishedEvents.Single() as ImportAgentEvent;
			@event.Should().Not.Be.Null();
			@event.JobResultId.Should().Be(job.Id);
			@event.Defaults.Should().Be(fallbacks);
		
		}

	}
}
