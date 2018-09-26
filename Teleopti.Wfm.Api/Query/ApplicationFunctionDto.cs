using System;

namespace Teleopti.Wfm.Api.Query
{
	public class ApplicationFunctionDto
	{
		public Guid Id { get; set; }
		public string FunctionCode { get; set; }
		public string FunctionPath { get; set; }
	}
}