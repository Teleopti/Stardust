namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class RequestsPermissonsViewModel
	{
		public RequestsPermissonsViewModel()
		{
			HasApproveOrDenyPermission = true;
			HasReplyPermission = true;
			HasEditSiteOpenHoursPermission = true;
			HasCancelPermission = true;
		}

		public bool HasApproveOrDenyPermission { get; set; }
		public bool HasReplyPermission { get; set; }
		public bool HasEditSiteOpenHoursPermission { get; set; }
		public bool HasCancelPermission { get; set; }
	}

	public class RequestLicenseAndPermissionViewModel
	{
		public bool IsOvertimeRequestEnabled { get; set; }
		public bool IsShiftTradeRequestEnabled { get; set; }
	}
}