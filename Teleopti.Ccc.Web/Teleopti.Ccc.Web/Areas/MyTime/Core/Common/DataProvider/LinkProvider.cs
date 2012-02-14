using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class LinkProvider : ILinkProvider
	{
		private readonly IResolve<UrlHelper> _urlHelper;

		public LinkProvider(IResolve<UrlHelper> urlHelper)
		{
			_urlHelper = urlHelper;
		}

		public string TextRequestLink(Guid value)
		{
			var urlHelper = _urlHelper.Invoke();
			var protocol = urlHelper.RequestContext.HttpContext.Request.Url.Scheme;
			return urlHelper.Action("TextRequest", "Requests", new { Id = value }, protocol);
		}
	}

}