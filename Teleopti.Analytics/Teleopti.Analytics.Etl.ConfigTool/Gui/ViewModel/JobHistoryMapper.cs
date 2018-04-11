using System;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public static class JobHistoryMapper
	{
		public static JobHistoryTreeViewModel Map(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors)
		{
			var historyModels = new JobHistoryRepository().GetEtlJobHistory(startDate, endDate, businessUnitId, showOnlyErrors,
				ConfigurationManager.AppSettings["datamartConnectionString"]);

			var returnList = new JobHistoryTreeViewModel();
			returnList.AddRange(historyModels);

			return returnList;
		}
	}
}
