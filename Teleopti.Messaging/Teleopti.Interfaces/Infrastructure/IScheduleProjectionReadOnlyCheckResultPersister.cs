using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public class ScheduleProjectionReadOnlyValidationResult
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public bool IsValid { get; set; }
	}


	public interface IScheduleProjectionReadOnlyCheckResultPersister
	{
		void Save(ScheduleProjectionReadOnlyValidationResult input);
		void Reset();
	}
}