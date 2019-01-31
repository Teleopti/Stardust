using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WorkerNode = Teleopti.Ccc.Infrastructure.Repositories.Stardust.WorkerNode;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class PingNode : IPingNode
	{
		private readonly HttpClient client;

		public PingNode()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public bool Ping(WorkerNode node)
		{
			return pingNode(node).Result;
		}

		private async Task<bool> pingNode(WorkerNode node)
		{
			var response = await client.GetAsync(node.Url + "ping/").ConfigureAwait(false);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return false;
			}

			return true;
		}
	}
}
