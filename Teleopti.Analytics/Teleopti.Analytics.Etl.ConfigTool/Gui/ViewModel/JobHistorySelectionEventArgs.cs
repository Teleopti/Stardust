using System;
using Teleopti.Analytics.Etl.Common.Entity;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistorySelectionEventArgs : EventArgs
	{
		public JobHistorySelectionEventArgs(DateTime startDate, DateTime endDate, BusinessUnitItem businessUnit, bool showOnlyErrors)
		{
			StartDate = startDate;
			EndDate = endDate;
			BusinessUnit = businessUnit;
			ShowOnlyErrors = showOnlyErrors;
		}

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public BusinessUnitItem BusinessUnit { get; set; }

		public bool ShowOnlyErrors { get; set; }
	}
}
