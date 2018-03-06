using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncInFatClientProcessEventPublisher : InProcessEventPublisher<IRunInSyncInFatClientProcess>
	{
		public SyncInFatClientProcessEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor) : base(resolver, processor)
		{
		}
	}
}
