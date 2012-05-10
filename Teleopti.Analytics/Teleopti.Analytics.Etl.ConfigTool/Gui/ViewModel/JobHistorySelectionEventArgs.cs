using System;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistorySelectionEventArgs : EventArgs
	{
		public JobHistorySelectionEventArgs(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors)
		{
			StartDate = startDate;
			EndDate = endDate;
			BusinessUnitId = businessUnitId;
			ShowOnlyErrors = showOnlyErrors;
		}

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public Guid BusinessUnitId { get; set; }

		public bool ShowOnlyErrors { get; set; }
	}
}
