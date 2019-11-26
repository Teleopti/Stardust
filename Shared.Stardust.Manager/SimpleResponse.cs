using System;
using Newtonsoft.Json;

namespace Shared.Stardust.Manager
{
    public class SimpleResponse
    {
        public enum Status
        {
            Ok,
            BadRequest,
            Conflict,
            InternalServerError,
            NotFound
        }

        public Status Result { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public object ResponseValue { get; }
      

        private SimpleResponse(Status result, string message = null, Exception exception = null, object responseValue = null)
        {
          
            Result = result;
            Message = message;
            Exception = exception;
            ResponseValue = responseValue;

        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static SimpleResponse Ok(object responseValue = null)
        {
            return  new SimpleResponse(Status.Ok, null, null, responseValue);
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

        public static SimpleResponse InternalServerError(Exception exception)
        {
            return  new SimpleResponse(Status.InternalServerError, null, exception);
        }
    }
}
