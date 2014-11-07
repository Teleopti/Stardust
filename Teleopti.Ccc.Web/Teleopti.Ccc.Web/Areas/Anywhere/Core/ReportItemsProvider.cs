using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportItemsProvider : IReportItemsProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IReportUrl _matrixWebsiteUrl;

		public ReportItemsProvider(IReportsProvider reportsProvider, ICurrentBusinessUnit currentBusinessUnit, IReportUrl matrixWebsiteUrl)
		{
			_reportsProvider = reportsProvider;
			_currentBusinessUnit = currentBusinessUnit;
			_matrixWebsiteUrl = matrixWebsiteUrl;
		}

		public List<ReportItem> GetReportItems()
		{
			var realBu = (Guid) _currentBusinessUnit.Current().Id;

			var reports = new List<IApplicationFunction>(_reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription));
			var reportItems = new List<ReportItem>();
			foreach (var applicationFunction in reports)
			{
				reportItems.Add(new ReportItem
				{
					Url = _matrixWebsiteUrl.Build(applicationFunction.ForeignId, realBu),
					Name = applicationFunction.LocalizedFunctionDescription,
				});
			}

			return reportItems;
		}
	}
}