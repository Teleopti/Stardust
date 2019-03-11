using System;

namespace Teleopti.Ccc.Domain.Status
{
	public class CustomStatusStep : IStatusStep
	{
		public const string Message = "{0} did some work {1} seconds ago";
		public const string MessageWhenDeadForLongTime = "{0} hasn't done any work for very long time";

		public CustomStatusStep(string name, string description, TimeSpan timeSinceLastPing, TimeSpan limit)
		{
			TimeSinceLastPing = timeSinceLastPing;
			Limit = limit;
			Name = name;
			Description = description;
		}
		
		public StatusStepResult Execute()
		{
			var result = TimeSinceLastPing <= Limit;
			var output = TimeSinceLastPing.TotalDays > 10
				? string.Format(MessageWhenDeadForLongTime, Name)
				: string.Format(Message, Name, TimeSinceLastPing.TotalSeconds);
			return new StatusStepResult(result, output);
		}

		public string Name { get; }
		public string Description { get; }
		public TimeSpan Limit { get; }
		public TimeSpan TimeSinceLastPing { get; }
	}
}