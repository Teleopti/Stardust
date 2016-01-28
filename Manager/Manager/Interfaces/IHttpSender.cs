using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stardust.Manager.Interfaces
{
	public interface IHttpSender
	{
		Task<HttpResponseMessage> PostAsync(string url, object data);

        Task<HttpResponseMessage> DeleteAsync(string url);
    }
}