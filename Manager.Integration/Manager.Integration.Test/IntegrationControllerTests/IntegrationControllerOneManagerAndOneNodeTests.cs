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
	[TestFixture]
	public class IntegrationControllerOneManagerAndOneNodeTests : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		private async Task<List<string>> GetAllManagers(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                                IHttpSender httpSender)
		{
			var managersUri =
				intergrationControllerUriBuilder.GetAllManagersUri();

			var request = await httpSender.GetAsync(managersUri);

			var result = request.Content.ReadAsStringAsync().Result;

			return JsonConvert.DeserializeObject(result,
			                                     typeof (List<string>)) as List<string>;
		}

		private async Task<List<string>> GetAllNodes(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                             IHttpSender httpSender)
		{
			var allNodesUri =
				intergrationControllerUriBuilder.GetAllNodesUri();

			var request = await httpSender.GetAsync(allNodesUri);

			var result = request.Content.ReadAsStringAsync().Result;

			return JsonConvert.DeserializeObject(result,
			                                     typeof (List<string>)) as List<string>;
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