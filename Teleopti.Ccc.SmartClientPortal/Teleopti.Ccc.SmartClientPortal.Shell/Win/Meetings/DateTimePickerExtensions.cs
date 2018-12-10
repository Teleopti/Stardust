using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	public static class DateTimePickerExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void SetSafeBoundary(this DateTimePickerAdv dateTimePickerAdv)
		{
			dateTimePickerAdv.MinValue = DateHelper.MinSmallDateTime;
			dateTimePickerAdv.MaxValue = DateHelper.MaxSmallDateTime;
		}
	}
}