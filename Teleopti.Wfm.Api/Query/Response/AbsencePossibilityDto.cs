using System;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class AbsencePossibilityDto
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public int Possibility { get; set; }
	}
}