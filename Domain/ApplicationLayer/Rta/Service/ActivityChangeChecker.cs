namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IActivityChangeChecker
	{
		void CheckForActivityChanges();
	}

	public class EmptyActivityChangeChecker : IActivityChangeChecker
	{
		public void CheckForActivityChanges()
		{
		}
	}

	public class ActivityChangeChecker : IActivityChangeChecker
	{
		private readonly IContextLoader _contextLoader;
		private readonly RtaProcessor _processor;

		public ActivityChangeChecker(
            IContextLoader contextLoader,
			RtaProcessor processor
			)
		{
			_contextLoader = contextLoader;
			_processor = processor;
		}

		public void CheckForActivityChanges()
		{
			_contextLoader.ForAll(person =>
			{
				_processor.Process(person);
			});
		}
	}
}