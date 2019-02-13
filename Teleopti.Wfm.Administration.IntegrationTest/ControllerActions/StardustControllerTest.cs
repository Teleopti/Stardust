using System;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[TestFixture]
	public class StardustControllerTest 
	{
		public StardustController Target;
		public IStardustRepository StardustRepository;
		private FakeTenants _LoadAllTenants;
		public FakeStardustSender Publisher;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			//TODO refactor to ioc
			_LoadAllTenants = new FakeTenants();
			Publisher = new FakeStardustSender();
			//StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			StardustRepository = new FakeStardustRepository();
			Target = new StardustController(StardustRepository, Publisher, _LoadAllTenants,
				new StaffingSettingsReader49Days(), new FakePingNode(),new FakeSkillForecastJobStartTimeRepository(new MutableNow(),new SkillForecastSettingsReader() ));
		}

		[Test]
		public void JobHistoryDetailsShouldNotCrash()
		{
			Target.JobHistoryDetails(Guid.NewGuid());
		}


		[Test]
		public void JobHistoryFilteredShouldNotCrash()
		{
			Target.JobHistoryFiltered(1,2);
		}


		[Test]
		public void QueuedJobShouldNotCrash()
		{
			Target.QueuedJob(Guid.NewGuid());
		}


		[Test]
		public void DeleteQueuedJobsShouldNotCrash()
		{
			Target.DeleteQueuedJobs(new[] {Guid.NewGuid()});
		}
		

		[Test]
		public void JobHistoryListByNodeShouldNotCrash()
		{
			Target.JobHistoryList(Guid.NewGuid(), 1, 10);
		}

		[Test]
		public void WorkerNodesShouldNotCrash()
		{
			Target.WorkerNodes();
		}

		[Test]
		public void AliveWorkerNodesShouldNotCrash()
		{
			Target.AliveWorkerNodes();
		}

		[Test]
		public void WorkerNodesWithIdShouldNotCrash()
		{
			Target.WorkerNodes(Guid.NewGuid());
		}

		[Test]
		public void GetFailedJobsFilteredShouldNotCrash()
		{
			Target.FailedJobHistoryFiltered(1, 10);
		}

		[Test]
		public void ShouldTriggerSkillForecastJob()
		{
		
			_LoadAllTenants.Has("Teleopti WFM");

			Target.TriggerSkillForecastCalculation(new SkillForecastCalculationModel()
			{
				StartDate = new DateTime(2019, 2, 13, 10, 0, 0),
				EndDate = new DateTime(2019, 2, 13, 10, 0, 0),
				Tenant = "Teleopti WFM"
			});

			Publisher.SentMessages.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);
		}
	}
}
 