using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[UseOnToggle(Toggles.RTA_ScaleOut_36979)]
	public class ActivityChangeProcessor : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
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

		[RecurringId("ActivityChangeProcessor:::TenantMinuteTickEvent")]
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