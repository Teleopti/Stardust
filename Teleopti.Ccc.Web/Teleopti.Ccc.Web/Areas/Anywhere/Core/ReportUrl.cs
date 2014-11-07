using System;
using System.Configuration;
using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportUrl : IReportUrl
	{

		public string Build(string foreignId, Guid businessId)
		{
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