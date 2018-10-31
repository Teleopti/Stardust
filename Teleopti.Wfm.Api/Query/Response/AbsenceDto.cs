using System;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class AbsenceDto
	{
		public Guid Id;
		public byte Priority;
		public string Name;
		public bool Requestable;
		public bool InWorkTime;
		public bool InPaidTime;
		public string PayrollCode;
		public bool Confidential;
	}
} 