using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels
{
	public class IntradayStaffingViewModel
	{
		public IntradayStaffingViewModel()
		{
			DataSeries = new StaffingDataSeries();
			StaffingHasData = false;
		}


		public StaffingDataSeries DataSeries { get; set; }

		public bool StaffingHasData { get; set; }
	}
}