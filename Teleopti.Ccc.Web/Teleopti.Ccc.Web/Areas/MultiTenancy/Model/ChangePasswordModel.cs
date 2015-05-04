using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class ChangePasswordModel
	{
		public Guid PersonId { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}
}