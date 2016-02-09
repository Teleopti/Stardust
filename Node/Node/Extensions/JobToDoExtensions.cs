using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Extensions
{
    public static class JobToDoExtensions
    {
        public static string SerializeToJson(this JobToDo jobToDo)
        {
            jobToDo.ThrowExceptionWhenNull();

            return JsonConvert.SerializeObject(jobToDo);
        }

        private static void ValidateJobDefinitionAndApiEndpoint(JobToDo jobToDo,
                                                                Uri apiEndpoint)
        {
            jobToDo.ThrowExceptionWhenNull();
            apiEndpoint.ThrowArgumentExceptionWhenNull();
        }


        public static Uri CreateUri(this JobToDo jobToDo,
                                    string endPoint)
        {
            jobToDo.ThrowExceptionWhenNull();
            endPoint.ThrowArgumentExceptionIfNullOrEmpty();

            Uri transformUri = new Uri(endPoint.Replace(NodeRouteConstants.JobIdOptionalParameter,
                                                        jobToDo.Id.ToString()));

            return transformUri;
        }

        public static async Task<HttpResponseMessage> PostAsync(this JobToDo jobToDo,
                                                                Uri apiEndpoint,
                                                                CancellationToken cancellationToken)
        {
            // Validate arguments.
            ValidateJobDefinitionAndApiEndpoint(jobToDo,
                                                apiEndpoint);

            // Call API.
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                DefineDefaultRequestHeaders(client);

                Uri uri = jobToDo.CreateUri(apiEndpoint.ToString());

                response = await client.PostAsync(uri,
                                                  null, 
                                                  cancellationToken);
            }

            return response;
        }

        private static void DefineDefaultRequestHeaders(HttpClient client)
        {
            // Validate.
            client.ThrowArgumentExceptionWhenNull();

            // Create request headers.
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ApplicationJson));
        }


        public static void ThrowExceptionWhenNull(this JobToDo jobToDo)
        {
            if (jobToDo == null)
            {
                throw new ArgumentException();
            }
        }
    }
}