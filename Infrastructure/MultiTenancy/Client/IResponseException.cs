using System.Net;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IResponseException
	{
		HttpStatusCode? ExceptionStatus(WebException wEx);
	}
}