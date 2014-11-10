using System;
using System.Configuration;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportUrl : IReportUrl
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public ReportUrl(ICurrentBusinessUnit currentBusinessUnit)
		{
			_currentBusinessUnit = currentBusinessUnit;
		}

		public string Build(string foreignId)
		{

			var businessId = (Guid)_currentBusinessUnit.Current().Id;

			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/"))
			{
				matrixWebsiteUrl += "/";
			}

			var url = string.Format(CultureInfo.CurrentCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
											matrixWebsiteUrl, foreignId, businessId);

			return url;
		}
	}
}