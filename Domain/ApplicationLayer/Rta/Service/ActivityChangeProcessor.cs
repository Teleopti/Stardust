namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor
	{
		private readonly IContextLoader _contextLoader;
		private readonly RtaProcessor _processor;

		public ActivityChangeProcessor(
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