using System;

namespace Teleopti.Wfm.Api.Command
{
	public class RemoveAbsenceDto : ICommandDto
	{
		public Guid PersonId;
		public DateTime PeriodStartUtc;
		public DateTime PeriodEndUtc;
		public Guid? ScenarioId;
	}
}