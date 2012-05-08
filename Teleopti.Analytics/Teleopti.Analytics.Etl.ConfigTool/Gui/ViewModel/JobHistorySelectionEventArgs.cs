using System;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistorySelectionEventArgs : EventArgs
	{
		public JobHistorySelectionEventArgs(DateTime startDate, DateTime endDate, Guid businessUnitId)
		{
			StartDate = startDate;
			EndDate = endDate;
			BusinessUnitId = businessUnitId;
		}

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public Guid BusinessUnitId { get; set; }
	}
}
