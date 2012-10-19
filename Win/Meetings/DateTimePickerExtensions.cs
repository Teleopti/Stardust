using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
{
	public static class DateTimePickerExtensions
	{
		public static void SetSafeBoundary(this DateTimePickerAdv dateTimePickerAdv)
		{
			dateTimePickerAdv.MinValue = DateHelper.MinSmallDateTime;
			dateTimePickerAdv.MaxValue = DateHelper.MaxSmallDateTime;
		}
	}
}