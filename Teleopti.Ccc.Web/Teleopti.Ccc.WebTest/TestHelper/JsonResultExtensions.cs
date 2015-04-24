using System.Web.Mvc;

namespace Teleopti.Ccc.WebTest.TestHelper
{
	public static class JsonResultExtensions
	{
		public static T Result<T>(this JsonResult jsonResult)
		{
			return (T) jsonResult.Data;
		}
	}
}