using System;
using System.Threading.Tasks;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Properties;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.Integration.Test.Helpers
{
	public static class IntegrationControllerApiHelper
	{
		public static async Task<string> ShutDownNode(HttpSender httpSender,
		                                              string nodeName)
		{
			var deleteUri = CreateUri(IntegrationControllerRouteConstants.NodeById.Replace("{id}", nodeName));

			var httpResponseMessage = await httpSender.DeleteAsync(deleteUri);

			var content = await httpResponseMessage.Content.ReadAsStringAsync();

			return content;
		}

		public static async Task<string> StartNewManager(HttpSender httpSender)
		{
			var allManagersUri = CreateUri(IntegrationControllerRouteConstants.Managers);

			var httpResponseMessage = await httpSender.PostAsync(allManagersUri);

			var content = await httpResponseMessage.Content.ReadAsStringAsync();

			return content;
		}

		public static async Task<string> StartNewNode(HttpSender httpSender)
		{
			var allNodesUri = CreateUri(IntegrationControllerRouteConstants.Nodes);

			var httpResponseMessage = await httpSender.PostAsync(allNodesUri);

			var content = await httpResponseMessage.Content.ReadAsStringAsync();

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