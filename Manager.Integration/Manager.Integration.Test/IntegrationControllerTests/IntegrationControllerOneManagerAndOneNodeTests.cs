using System.Collections.Generic;
using System.Threading.Tasks;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test.IntegrationControllerTests
{
	[TestFixture,Ignore]
	public class IntegrationControllerOneManagerAndOneNodeTests : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		private async Task<List<string>> GetAllManagers(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                                IHttpSender httpSender)
		{
			var managersUri =
				intergrationControllerUriBuilder.GetAllManagersUri();

			var httpResponseMessage =
				await httpSender.GetAsync(managersUri);

			var result =
				httpResponseMessage.Content.ReadAsStringAsync().Result;

			return JsonConvert.DeserializeObject(result,
			                                     typeof (List<string>)) as List<string>;
		}

		private async Task<string> ShutDownManager(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                           IHttpSender httpSender,
		                                           string managerName)
		{
			var deleteUri =
				intergrationControllerUriBuilder.GetManagerUriByManagerName(managerName);

			var httpResponseMessage = httpSender.DeleteAsync(deleteUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		private async Task<string> ShutDownNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                        IHttpSender httpSender,
		                                        string nodeName)
		{
			var deleteUri =
				intergrationControllerUriBuilder.GetNodeUriByNodeName(nodeName);

			var httpResponseMessage = httpSender.DeleteAsync(deleteUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		private async Task<string> StartNewManager(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                           IHttpSender httpSender)
		{
			var allManagersUri =
				intergrationControllerUriBuilder.GetAllManagersUri();

			var httpResponseMessage = httpSender.PostAsync(allManagersUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		private async Task<string> StartNewNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                        IHttpSender httpSender)
		{
			var allNodesUri =
				intergrationControllerUriBuilder.GetAllNodesUri();

			var httpResponseMessage = httpSender.PostAsync(allNodesUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		private async Task<List<string>> GetAllNodes(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                             IHttpSender httpSender)
		{
			var allNodesUri =
				intergrationControllerUriBuilder.GetAllNodesUri();

			var httpResponseMessage =
				await httpSender.GetAsync(allNodesUri);

			var result =
				httpResponseMessage.Content.ReadAsStringAsync().Result;

			return JsonConvert.DeserializeObject(result,
			                                     typeof (List<string>)) as List<string>;
		}

		[Test]
		public void ShouldBeAbleToShutDownManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = ShutDownManager(intergrationControllerUriBuilder,
			                               httpSender,
			                               "Manager1.config");

			Assert.IsNotNull(nodeName, "Should shut down manager.");
		}

		[Test]
		public void ShouldBeAbleToShutDownNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = ShutDownNode(intergrationControllerUriBuilder,
			                            httpSender,
			                            "Node1.config");

			Assert.IsNotNull(nodeName, "Should shut down node.");
		}

		[Test]
		public void ShouldBeAbleToStartNewManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var managerName = StartNewManager(intergrationControllerUriBuilder,
			                                  httpSender);

			Assert.IsNotNull(managerName, "Should start up a new manager.");
		}

		[Test]
		public void ShouldBeAbleToStartNewNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = StartNewNode(intergrationControllerUriBuilder,
			                            httpSender);

			Assert.IsNotNull(nodeName, "Should start up a new node.");
		}

		[Test]
		public void ShouldHaveStartedOneManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			// Get Managers.
			var allManagers = GetAllManagers(intergrationControllerUriBuilder,
			                                 httpSender);

			Assert.IsTrue(allManagers.Result.Count == 1, "Should return one manager.");
		}

		[Test]
		public void ShouldHaveStartedOneNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			// Get Nodes.
			var allNodes = GetAllNodes(intergrationControllerUriBuilder,
			                           httpSender);

			Assert.IsTrue(allNodes.Result.Count == 1, "Should return one node.");
		}
	}
}