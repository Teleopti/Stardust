using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class PingNode : IPingNode
	{
		public bool Ping(WorkerNode node)
		{
			return pingNode(node).Result;
		}

		private static async Task<bool> pingNode(WorkerNode node)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.PostAsync(node.Url + "ping/", null).ConfigureAwait(false);
				if (response.StatusCode != HttpStatusCode.OK)
				{
					return false;
				}
			}
			return true;
		}
	}

	public class FakePigNode : IPingNode
	{
		public bool Ping(WorkerNode node)
		{
			return true;
		}
	}
}
