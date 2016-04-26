//using System;
//using System.Configuration;
//using System.Threading;
//using ManagerTest.Database;
//using NUnit.Framework;
//using Stardust.Manager;
//using Stardust.Manager.Models;

//namespace ManagerTest
//{
//	[TestFixture]
//	public class WorkerNodeRepositoryTest : DatabaseTest
//	{
//		public WorkerNodeRepository WorkerNodeRepository { get; set; }

//		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");

//		[TestFixtureSetUp]
//		public void TestFixtureSetUp()
//		{
//			WorkerNodeRepository = 
//				new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, 
//										 new RetryPolicyProvider());
//		}
		

		
//	}
//}