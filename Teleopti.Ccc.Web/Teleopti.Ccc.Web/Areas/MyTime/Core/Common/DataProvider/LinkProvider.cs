using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class LinkProvider : ILinkProvider
	{
		private readonly IUrlHelper _urlHelper;

		public LinkProvider(IUrlHelper urlHelper)
		{
			_urlHelper = urlHelper;
		}

		public string RequestDetailLink(Guid value)
		{
			var urlHelper = _urlHelper.Fetch();
			return urlHelper.Action("RequestDetail", "Requests", new { Id = value });
		}
	}
}