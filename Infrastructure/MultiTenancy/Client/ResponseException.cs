using System.Net;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class ResponseException : IResponseException
	{
		public HttpStatusCode? ExceptionStatus(WebException wEx)
		{
			if (wEx.Status == WebExceptionStatus.ProtocolError)
			{
				return ((HttpWebResponse) wEx.Response).StatusCode;
			}
			return null;
		}
	}
}
