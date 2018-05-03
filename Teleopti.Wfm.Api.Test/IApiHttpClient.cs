using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Wfm.Api.Test
{
	public interface IApiHttpClient
	{
		Task<HttpResponseMessage> GetAsync(string requestUri);
		Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
		void Authorize();
	}
}