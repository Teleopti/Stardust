namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IActivityChangeCheckerFromScheduleChangeProcessor
	{
		void CheckForActivityChanges();
	}

	public class DontCheckForActivityChangesFromScheduleChangeProcessor : IActivityChangeCheckerFromScheduleChangeProcessor
	{
		public void CheckForActivityChanges()
		{
		}
	}

	public class ActivityChangeChecker : IActivityChangeCheckerFromScheduleChangeProcessor
	{
		private readonly IContextLoader _contextLoader;

		public ActivityChangeChecker(IContextLoader contextLoader)
		{
			_contextLoader = contextLoader;
		}

		public void CheckForActivityChanges()
		{
			_contextLoader.ForActivityChanges();
		}
	}
}