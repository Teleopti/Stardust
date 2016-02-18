using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Stardust.Manager.Interfaces;

namespace ManagerTest.Fakes
{
    public class FakeHttpSender : IHttpSender
    {
        public List<string> BusyNodesUrl { get; set; }
        public Dictionary<string, object> CalledNodes { get; set; }
        public List<HttpResponseMessage> Responses { get; set; }

        public FakeHttpSender()
        {
            BusyNodesUrl = new List<string>();
            CalledNodes = new Dictionary<string, object>();

            Responses = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.OK)
            };
        }


#pragma warning disable 1998
        public async Task<HttpResponseMessage> PostAsync(Uri url,
#pragma warning restore 1998
                                                         object data)
        {
            if (BusyNodesUrl.Any(url.ToString().Contains))
            {
                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }

            CalledNodes.Add(url.ToString(),
                            data);

            if (Responses.Count == CalledNodes.Count())
            {
                return Responses[CalledNodes.Count - 1];
            }

            return Responses[0];
        }
#pragma warning disable 1998
        public async Task<HttpResponseMessage> DeleteAsync(Uri url)
#pragma warning restore 1998
        {
            CalledNodes.Add(url.ToString(), null);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public Task<HttpResponseMessage> GetAsync(Uri url)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryGetAsync(Uri url)
        {
            throw new NotImplementedException();
        }
    }
}