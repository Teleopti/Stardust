using System;

namespace Teleopti.Wfm.Administration.Models
{
	public class ImportUserModel
	{
		public string AppLogon { get; set; }
		public string AppPassword { get; set; }
		public Guid PersonId { get; set; }
		public string Identity { get; set; }
	}
}