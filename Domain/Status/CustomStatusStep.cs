using System;

namespace Teleopti.Ccc.Domain.Status
{
	public class CustomStatusStep : IStatusStep
	{
		private readonly ITimeSinceLastPing _timeSinceLastPing;
		private readonly TimeSpan _limit;
		public const string Message = "{0} did some work {1} seconds ago";
		public const string MessageWhenDeadForLongTime = "{0} hasn't done any work for very long time";

		public CustomStatusStep(string name, string description, ITimeSinceLastPing timeSinceLastPing, TimeSpan limit)
		{
			_timeSinceLastPing = timeSinceLastPing;
			_limit = limit;
			Name = name;
			Description = description;
		}
		
		public StatusStepResult Execute()
		{
			var timeSinceLastPing = _timeSinceLastPing.Execute(this);
			var result = timeSinceLastPing <= _limit;
			var output = timeSinceLastPing.TotalDays > 1
				? string.Format(MessageWhenDeadForLongTime, Name)
				: string.Format(Message, Name, timeSinceLastPing.TotalSeconds);
			return new StatusStepResult(result, output);
		}

		public string Name { get; }
		public string Description { get; }
	}
}