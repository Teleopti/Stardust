using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class FakeStardustAndRunInProcess : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;

		public FakeStardustAndRunInProcess(ResolveEventHandlers resolver, 
			CommonEventProcessor processor,
			ICurrentPrincipalContext currentPrincipalContext)
		{
			_resolver = resolver;
			_processor = processor;
			_currentPrincipalContext = currentPrincipalContext;
		}

		public void Publish(params IEvent[] events)
		{
			Task.WaitAll((
					from @event in events
#pragma warning disable 618
					from handlerType in _resolver.HandlerTypesFor<IRunOnStardust>(@event)
					select Task.Run(() =>
						{
							//to simulate going to new process/being "logged out"
							_currentPrincipalContext.SetCurrentPrincipal(null);
							//
							_processor.Process(@event, handlerType);
						}
#pragma warning restore 618
					))
				.ToArray());
		}
	}
}