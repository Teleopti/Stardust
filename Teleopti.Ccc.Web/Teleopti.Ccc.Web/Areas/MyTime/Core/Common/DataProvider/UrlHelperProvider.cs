using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class UrlHelperProvider : IUrlHelper
	{
		public UrlHelper Fetch()
		{
			return DependencyResolver.Current.GetService<UrlHelper>();
		}
	}
}