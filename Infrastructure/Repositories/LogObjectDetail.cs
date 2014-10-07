using System;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LogObjectDetail
	{
		public string log_object_desc { get; set; }
		public int log_object_id { get; set; }
		public string detail_desc { get; set; }
		public string proc_name { get; set; }
		public DateTime last_update { get; set; }
	}
}