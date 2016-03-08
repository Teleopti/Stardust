using System;
using System.Configuration;
using NUnit.Framework;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TestFixture]
	class StardustControllerTest
	{
		public StardustController Target;
		public StardustHelper StardustHelper;
		public StardustRepository StardustRepository;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			StardustHelper = new StardustHelper(StardustRepository);
			Target = new StardustController(StardustHelper);
		}

		[Test]
		public void JobHistoryDetailsShouldNotCrash()
		{
			Target.JobHistoryDetails(Guid.NewGuid());
		}

		[Test]
		public void JobHistoryListShouldNotCrash()
		{
			Target.JobHistoryList();
		}

		[Test]
		public void JobHistoryShouldNotCrash()
		{
			Target.JobHistory(Guid.NewGuid());
		}

		[Test]
		public void WorkerNodesShouldNotCrash()
		{
			Target.WorkerNodes();
		}

		[Test]
		public void WorkerNodesWithIdShouldNotCrash()
		{
			Target.WorkerNodes(Guid.NewGuid());
		}
	}
}