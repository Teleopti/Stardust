using System;
using System.Threading.Tasks;
using Manager.IntegrationTest.Constants;
using Manager.IntegrationTest.Properties;
using Manager.IntegrationTest.ConsoleHost.Helpers;

namespace Manager.IntegrationTest.Helpers
{
	public static class IntegrationControllerApiHelper
	{
		public static async Task<string> ShutDownNode(HttpSender httpSender,
		                                              string nodeName)
		{
			var deleteUri = CreateUri(IntegrationControllerRouteConstants.NodeById.Replace("{id}", nodeName));

			var httpResponseMessage = httpSender.DeleteAsync(deleteUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		public static async Task<string> StartNewManager(HttpSender httpSender)
		{
			var allManagersUri = CreateUri(IntegrationControllerRouteConstants.Managers);

			var httpResponseMessage = httpSender.PostAsync(allManagersUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		public static async Task<string> StartNewNode(HttpSender httpSender)
		{
			var allNodesUri = CreateUri(IntegrationControllerRouteConstants.Nodes);

			var httpResponseMessage = httpSender.PostAsync(allNodesUri);

			httpResponseMessage.Wait();

			var res = httpResponseMessage.Result;

			var content = await res.Content.ReadAsStringAsync();

			return content;
		}

		public static Uri CreateUri(string path)
		{
			var uriBuilder = new UriBuilder(new Uri(Settings.Default.IntegrationControllerBaseAddress));

		//	_uriBuilder.Path = _uriTemplateBuilder.Path;

			uriBuilder.Path += path;

			return uriBuilder.Uri;
		}
	}
}