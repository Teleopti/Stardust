using System.Collections.Generic;

namespace Teleopti.Wfm.Api
{
	public class QueryResultDto<T>
	{
		public IEnumerable<T> Result { get; set; }
		public bool Successful { get; set; }
		public string Message { get; set; }
	}
}