using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class FakeStardustAndRunInProcess : InProcessEventPublisher<IRunOnStardust>
	{
		public FakeStardustAndRunInProcess(ResolveEventHandlers resolver, CommonEventProcessor processor) : base(resolver, processor)
		{
		}
	}
}