using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public static class BusinessUnitItemMapper
	{
		public static IList<BusinessUnitItem> Map()
		{
			var repository = new Repository(ConfigurationManager.AppSettings["datamartConnectionString"]);

			var dataTable = repository.BusinessUnitsIncludingAllItem;

			return dataTable == null ? null : (from DataRow row in dataTable.Rows select new BusinessUnitItem {Id = (Guid) row["id"], Name = (string) row["name"]}).ToList();
		}
	}
}
