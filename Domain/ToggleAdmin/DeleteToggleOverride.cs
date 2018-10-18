namespace Teleopti.Ccc.Domain.ToggleAdmin
{
	public class DeleteToggleOverride
	{
		private readonly IPersistToggleOverride _persistToggleOverride;

		public DeleteToggleOverride(IPersistToggleOverride persistToggleOverride)
		{
			_persistToggleOverride = persistToggleOverride;
		}
		public void Execute(string toggle)
		{
			_persistToggleOverride.Delete(toggle);
		}
	}
}