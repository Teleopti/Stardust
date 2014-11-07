using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportItemsProvider : IReportItemsProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;

		public ReportItemsProvider(IReportsProvider reportsProvider, ISessionSpecificDataProvider sessionSpecificDataProvider)
		{
			_reportsProvider = reportsProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
		}

		public List<ReportItem> GetReportItems()
		{
			var data = _sessionSpecificDataProvider.GrabFromCookie();
			var realBu = data.BusinessUnitId;
			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";

			var reports = new List<IApplicationFunction>(_reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription));
			var reportItems = new List<ReportItem>();
			foreach (var applicationFunction in reports)
			{
				var url = string.Format(CultureInfo.CurrentCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
					matrixWebsiteUrl, applicationFunction.ForeignId, realBu);

				reportItems.Add(new ReportItem
				{
					Url = url,
					Name = applicationFunction.LocalizedFunctionDescription,
				});
			}

			return reportItems;
		}
	}
}