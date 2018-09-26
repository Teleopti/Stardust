namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class RequestPermission
	{
		public bool TextRequestPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool ShiftTradeRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
		public bool PersonAccountPermission { get; set; }
		public bool OvertimeRequestPermission { get; set; }
		public bool RequestListPermission { get; set; }
	}
}