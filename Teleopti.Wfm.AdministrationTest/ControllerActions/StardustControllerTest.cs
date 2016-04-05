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
		public StardustRepository StardustRepository;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			Target = new StardustController(StardustRepository);
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
		public void JobHistoryListByNodeShouldNotCrash()
		{
			Target.JobHistoryList(Guid.NewGuid());
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