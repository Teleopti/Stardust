using System;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class WindowsLogonInfo
	{
		public Guid PersonCode { get; set; }
		public string WindowsDomain { get; set; }
		public string WindowsUsername { get; set; }
	}
}