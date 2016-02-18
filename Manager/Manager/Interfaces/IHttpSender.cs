using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stardust.Manager.Interfaces
{
    public interface IHttpSender
    {
        Task<HttpResponseMessage> PostAsync(Uri url,
                                            object data);

        Task<HttpResponseMessage> DeleteAsync(Uri url);

        Task<HttpResponseMessage> GetAsync(Uri url);

        Task<bool> TryGetAsync(Uri url);
    }
}