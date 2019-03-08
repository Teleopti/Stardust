namespace Teleopti.Ccc.Domain.Status
{
	public class CustomStatusStep : IStatusStep
	{
		public CustomStatusStep(string name, string description)
		{
			Name = name;
			Description = description;
		}
		
		public StatusStepResult Execute()
		{
			throw new System.NotImplementedException();
		}

		public string Name { get; }
		public string Description { get; }
	}
}