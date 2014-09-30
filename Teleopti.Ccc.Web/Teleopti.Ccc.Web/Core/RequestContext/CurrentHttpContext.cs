using System;
using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentHttpContext : ICurrentHttpContext
	{
		public HttpContextBase Current()
		{
			try
			{
				return new HttpContextWrapper(HttpContext.Current);
			}
			catch (ArgumentNullException)
			{
				return null;
			}
		}
	}
}