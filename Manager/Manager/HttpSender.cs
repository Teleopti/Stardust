﻿using System;
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start.");

            try
            {
                using (var client = new HttpClient())
                {
                    string sez = JsonConvert.SerializeObject(data);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response =
                        await client.PostAsync(url,
                                               new StringContent(sez,
                                                                 Encoding.Unicode,
                                                                 "application/json"));

                    LogHelper.LogInfoWithLineNumber(Logger,
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start.");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.DeleteAsync(url);

                    LogHelper.LogInfoWithLineNumber(Logger,
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                                   "GetAsync Start");
            try
            {
                using (var client = new HttpClient())
                {
                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "start using Httpclient");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "GetAsync with" + url);
                    var response = await client.GetAsync(url);

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "OK");
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                                    "TryGetAsync Start");
            try
            {
                using (var client = new HttpClient())
                {
                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "start using Httpclient");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "before GetAsync");

                    var response = await GetAsync(url);
                    //      var response = await client.GetAsync(url);

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "OK");
                    return response.IsSuccessStatusCode;
                }
            }

            catch (HttpRequestException exp)
            {
                LogHelper.LogWarningWithLineNumber(Logger,
                                                   exp.Message);

            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 exp.Message,
                                                 exp);

                throw;
            }
            LogHelper.LogInfoWithLineNumber(Logger,
                                                    "Return false");
            return false;
        }
    }
}