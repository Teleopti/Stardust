using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class LogonInfoModel
	{
		public Guid PersonId { get; set; }
		public string LogonName { get; set; }
		public string Identity { get; set; }
	}
}