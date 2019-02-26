using System;
using Teleopti.Ccc.Domain.ETL;

namespace Teleopti.Ccc.Domain.Status
{
	public class EtlChecker : IStatusStep
	{
		private readonly ITimeSinceLastEtlPing _timeSinceLastEtlPing;
		public static readonly string Message = "ETL did some work {0} seconds ago";
		public const string MessageWhenDeadForLongTime = "ETL hasn't done any work for very long time";

		public EtlChecker(ITimeSinceLastEtlPing timeSinceLastEtlPing)
		{
			_timeSinceLastEtlPing = timeSinceLastEtlPing;
		}
		
		public StatusStepResult Execute()
		{
			var timeSinceLastEtlPing = _timeSinceLastEtlPing.Fetch();
			return new StatusStepResult(
				timeSinceLastEtlPing < EtlTickFrequency.Value + TimeSpan.FromSeconds(30), 
				timeSinceLastEtlPing.TotalDays > 1 ? MessageWhenDeadForLongTime : string.Format(Message, timeSinceLastEtlPing));
		}

		public string Name { get; } = "ETL";
		public string Description { get; } = "Verifies ETL service is alive";
	}
}