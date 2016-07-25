using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor : 
		IHandleEvent<TenantMinuteTickEvent>, 
		IRunOnHangfire
	{
		private readonly ContextLoader _contextLoader;
		private readonly RtaProcessor _processor;
		private readonly IDistributedLockAcquirer _distributedLock;

		public ActivityChangeProcessor(
			ContextLoader contextLoader,
			RtaProcessor processor,
			IDistributedLockAcquirer distributedLock
			)
		{
			_contextLoader = contextLoader;
			_processor = processor;
			_distributedLock = distributedLock;
		}

		[RecurringJob]
		[ReadModelUnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, CheckForActivityChanges);
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