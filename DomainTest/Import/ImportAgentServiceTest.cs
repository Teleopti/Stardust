﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.DomainTest.Logon;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Import
{
	[TestFixture, DomainTest]
	public class ImportAgentServiceTest : ISetup
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
			setCurrentLoggedOnUser();
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
			var person1 = setCurrentLoggedOnUser(bu1);
			createValidJob();
			var bu2 = BusinessUnitFactory.CreateWithId("bu2");
			var person2 = setCurrentLoggedOnUser(bu2);
			createValidJob();
			var person3 = setCurrentLoggedOnUser(bu2);
			createValidJob();

			var list = Target.GetJobsForLoggedOnBusinessUnit();
			list.Count.Should().Be(2);
			list.Any(j => j.Owner.Id == person1.Id).Should().Be.False();
			list.Any(j => j.Owner.Id == person2.Id).Should().Be.True();
			list.Any(j => j.Owner.Id == person3.Id).Should().Be.True();
		}

		[Test]
		public void ShouldGetCorrentJobsDetailWhenJobIsNotFinished()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu1");
			var person1 = setCurrentLoggedOnUser(bu1);
			var job1 = createValidJob();
			var detail = Target.GetJobsForLoggedOnBusinessUnit().FirstOrDefault();

			detail.IsWorking.Should().Be(false);
		
		}
	

		private IPerson setCurrentLoggedOnUser(BusinessUnit bu = null)
		{
			var person = PersonFactory.CreatePersonWithId();
			if (bu != null)
			{
				person.WorkflowControlSet = new WorkflowControlSet();
				person.WorkflowControlSet.SetBusinessUnit(bu);
			}
			LoggedOnUser.SetFakeLoggedOnUser(person);
			return person;
		}


		private IJobResult createValidJob(ImportAgentDefaults fallbacks = null)
		{
			var fileData = new FileData()
			{
				FileName = "test.xlsx",
				Data = Encoding.ASCII.GetBytes("test")
			};
			return Target.CreateJob(fileData, fallbacks);
		}
	}
}
