using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
	{
		private readonly ContextLoader _contextLoader;
		private readonly RtaProcessor _processor;

		public ActivityChangeProcessor(
			ContextLoader contextLoader,
			RtaProcessor processor
			)
		{
			_contextLoader = contextLoader;
			_processor = processor;
		}

		[RecurringJob]
		public void Handle(TenantMinuteTickEvent @event)
		{
			CheckForActivityChanges();
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