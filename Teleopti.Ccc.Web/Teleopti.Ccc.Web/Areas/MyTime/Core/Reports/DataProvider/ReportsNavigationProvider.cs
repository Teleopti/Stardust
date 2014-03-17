using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public class ReportsNavigationProvider: IReportsNavigationProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly IReportsProvider _reportsProvider;

		public ReportsNavigationProvider(IPrincipalAuthorization principalAuthorization, IReportsProvider reportsProvider)
		{
			_principalAuthorization = principalAuthorization;
			_reportsProvider = reportsProvider;
		}

		public IList<ReportNavigationItem> GetNavigationItems()
		{
			var fakeMatrixWebsiteUrl = "http://www.teleopti.com?ReportId={0}&ForceForms={1}&BuId={2}";
			var fakeBu = Guid.NewGuid();
			var forceFormsLogin = false;
			var reportsList = new List<ReportNavigationItem>();
			if (_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb))
				reportsList.Add(new ReportNavigationItem
				{
					Action = "Index",
					Controller = "MyReport",
					Title = Resources.MyReport,
					IsMyReport = true
				});
			var otherReports = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);
			if (otherReports.Any()&& reportsList.Any())
				reportsList.Add(new ReportNavigationItem
				{
					IsDivider = true
				});
			foreach (var applicationFunction in otherReports)
			{
				var url = string.Format(CultureInfo.CurrentCulture, fakeMatrixWebsiteUrl,
													 applicationFunction.ForeignId,
													 forceFormsLogin, fakeBu);

				reportsList.Add(new ReportNavigationItem
				{
					Url = url,
					Title = applicationFunction.LocalizedFunctionDescription,
				});
			}
			return reportsList;
		}
	}

}