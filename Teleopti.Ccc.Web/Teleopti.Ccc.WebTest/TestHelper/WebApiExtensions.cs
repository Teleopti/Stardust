using System.Web.Http;
using System.Web.Http.Results;

namespace Teleopti.Ccc.WebTest.TestHelper
{
	public static class WebApiExtensions
	{
		public static T OkContent<T>(this IHttpActionResult actionResult)
		{
			var action = (OkNegotiatedContentResult<T>)actionResult;
			return action.Content;
		}
	}
}