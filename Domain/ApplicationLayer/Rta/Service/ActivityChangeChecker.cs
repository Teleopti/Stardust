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
		private readonly RtaProcessor _processor;

		public ActivityChangeChecker(IContextLoader contextLoader, RtaProcessor processor)
		{
			_contextLoader = contextLoader;
			_processor = processor;
		}

		public void CheckForActivityChanges()
		{
			_contextLoader.ForActivityChanges(person =>
			{
				_processor.Process(person);
			});
		}
	}
}