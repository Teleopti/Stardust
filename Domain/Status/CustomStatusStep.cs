namespace Teleopti.Ccc.Domain.Status
{
	public class CustomStatusStep : IStatusStep
	{
		private readonly ITimeSinceLastPing _timeSinceLastPing;

		public CustomStatusStep(string name, string description, ITimeSinceLastPing timeSinceLastPing)
		{
			_timeSinceLastPing = timeSinceLastPing;
			Name = name;
			Description = description;
		}
		
		public StatusStepResult Execute()
		{
			_timeSinceLastPing.Execute(this);
			return null;
		}

		public string Name { get; }
		public string Description { get; }
	}
}