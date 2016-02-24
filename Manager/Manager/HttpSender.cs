using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;

namespace Stardust.Manager
{
    public class HttpSender : IHttpSender
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (HttpSender));

        public async Task<HttpResponseMessage> PostAsync(Uri url,
                                                         object data)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Start.");

            try
            {
                using (var client = new HttpClient())
                {
					var sez = JsonConvert.SerializeObject(data);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response =
                        await client.PostAsync(url,
                                               new StringContent(sez,
                                                                 Encoding.Unicode,
                                                                 "application/json"))
                            .ConfigureAwait(false);

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Finished.");

                    return response;
                }
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);

                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(Uri url)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Start.");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.DeleteAsync(url)
                            .ConfigureAwait(false);

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Finished.");

                    return response;
                }
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);

                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(Uri url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    var response = await client.GetAsync(url,
											  HttpCompletionOption.ResponseHeadersRead)
                            .ConfigureAwait(false);
                    
                    return response;
                }
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);

                throw;
            }
        }

        public async Task<bool> TryGetAsync(Uri url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response =
                        await client.GetAsync(url,
                                              HttpCompletionOption.ResponseHeadersRead)
                            .ConfigureAwait(false);
                    
                    return response.IsSuccessStatusCode;
                }
            }

            catch (HttpRequestException exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                   exp.Message,
												   exp);
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);

                throw;
            }
            LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Return false");
            return false;
        }
    }
}