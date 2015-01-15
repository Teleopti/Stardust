using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class IdentificationResult
	{
		public LogonResult Result { get; set; }
		public Guid PersonId { get; set; }
		public string Tennant { get; set; }
	}
}