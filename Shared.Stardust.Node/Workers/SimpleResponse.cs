using System;
using Newtonsoft.Json;

namespace Shared.Stardust.Node.Workers
{
    public class SimpleResponse
    {
        public enum Status
        {
            Ok,
            BadRequest,
            Conflict,
            NotFound
        }

        public Status Result { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public object ResponseValue { get; }

        public SimpleResponse(Status result, string message = null, Exception exception = null, string responseValue = null)
        {
            Result = result;
            Message = message;
            Exception = exception;
            ResponseValue = responseValue;

        }

        private static string SerializeObject(object obj)
        {
            //Type t = obj.GetType();
            return JsonConvert.SerializeObject(obj);
        }

        public static SimpleResponse Ok(object responseValue = null)
        {
            var responseString = responseValue == null ?  "" : SerializeObject(responseValue);
            return  new SimpleResponse(Status.Ok, null, null, responseString);
        }

        public static SimpleResponse BadRequest(string message)
        {
            return  new SimpleResponse(Status.BadRequest, message);
        }

        public static SimpleResponse NotFound()
        {
            return  new SimpleResponse(Status.NotFound);
        }

        public static SimpleResponse Conflict()
        {
            return  new SimpleResponse(Status.Conflict);
        }

    }
}
