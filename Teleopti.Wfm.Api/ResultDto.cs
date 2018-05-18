using System;

namespace Teleopti.Wfm.Api
{
	public class ResultDto
	{
		public bool Successful { get; set; }
		public Guid? Id { get; set; }
		public string Message { get; set; }
	}
}