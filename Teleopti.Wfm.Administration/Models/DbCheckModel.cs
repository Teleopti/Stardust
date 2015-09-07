using System.Runtime.Remoting.Channels;

namespace Teleopti.Wfm.Administration.Models
{
	public class DbCheckModel
	{
		public string Server { get; set; }

		public string AdminUser { get; set; }

		public string AdminPassword { get; set; }
		public string UserName { get; set; }

		public string Password { get; set; }
		public string Database { get; set; }
		//1 = app 2 = analytics
		public int DbType { get; set; }
	}
}