namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeChecker
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