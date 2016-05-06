using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.Integration.Test.Helpers
{
	public static class IntegrationControllerApiHelper
	{

		public static async Task<string> ShutDownNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                              HttpSender httpSender,
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

		public static async Task<string> StartNewManager(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                                 HttpSender httpSender)
		{
			var allManagersUri =
				intergrationControllerUriBuilder.GetAllManagersUri();

			var httpResponseMessage = httpSender.PostAsync(allManagersUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		public static async Task<string> StartNewNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
		                                              HttpSender httpSender)
		{
			var allNodesUri =
				intergrationControllerUriBuilder.GetAllNodesUri();

			var httpResponseMessage = httpSender.PostAsync(allNodesUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}
	}
}