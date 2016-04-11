using System.Collections.Generic;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
	public static class IntegrationControllerApiHelper
	{
		public static async Task<List<string>> GetAllManagers(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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

		public static async Task<string> ShutDownManager(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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

		public static async Task<string> ShutDownNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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

		public static async Task<string> StartNewManager(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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

		public static async Task<string> StartNewNode(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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

		public static async Task<List<string>> GetAllNodes(IntergrationControllerUriBuilder intergrationControllerUriBuilder,
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
	}
}