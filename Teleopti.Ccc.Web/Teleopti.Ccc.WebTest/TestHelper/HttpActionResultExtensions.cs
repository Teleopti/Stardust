using System.Web.Http;
using System.Web.Http.Results;

namespace Teleopti.Ccc.WebTest.TestHelper
{
	public static class HttpActionResultExtensions
	{
		public static T Result<T>(this IHttpActionResult actionResult)
		{
			return ((OkNegotiatedContentResult<T>)actionResult).Content;
		}
	}
}