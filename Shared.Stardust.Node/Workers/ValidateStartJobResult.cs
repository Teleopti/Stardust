using System.Net.Http;

namespace Stardust.Node.Workers
{
    public class ValidateStartJobResult
    {
        public ValidateStartJobResult(HttpResponseMessage httpResponseMessage, bool isWorking)
        {
            HttpResponseMessage = httpResponseMessage;
            IsWorking = isWorking;
        }
        
        public readonly HttpResponseMessage HttpResponseMessage;
        public readonly bool IsWorking;
    }
}