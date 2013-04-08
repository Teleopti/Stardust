using System;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class LinkProvider : ILinkProvider
	{
		private readonly Func<UrlHelper> _urlHelper;

		public LinkProvider(Func<UrlHelper> urlHelper)
		{
			_urlHelper = urlHelper;
		}

		public string RequestDetailLink(Guid value)
		{
			var protocol = _urlHelper().RequestContext.HttpContext.Request.Url.Scheme;
			return _urlHelper().Action("RequestDetail", "Requests", new { Id = value }, protocol);
		}
	}
}