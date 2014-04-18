using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal
{
	public class LogOnDetails
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public DataSourceDto DataSource { get; set; }
	}
}