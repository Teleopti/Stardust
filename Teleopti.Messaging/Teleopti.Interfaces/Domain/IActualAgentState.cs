using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IActualAgentState
	{
		Guid PersonId { get; set; }
		Guid BusinessUnitId { get; set; }
		string State { get; set; }
		Guid? StateId { get; set; }
		string Scheduled { get; set; }
		Guid? ScheduledId { get; set; }
		DateTime? StateStart { get; set; }
		string ScheduledNext { get; set; }
		Guid? ScheduledNextId { get; set; }
		DateTime? NextStart { get; set; }
		string AlarmName { get; set; }
		Guid? AlarmId { get; set; }
		int? Color { get; set; }
		DateTime? AlarmStart { get; set; }
		double? StaffingEffect { get; set; }
		string StateCode { get; set; }
		Guid PlatformTypeId { get; set; }
		DateTime ReceivedTime { get; set; }
		DateTime? BatchId { get; set; }
		string OriginalDataSourceId { get; set; }
	}
}