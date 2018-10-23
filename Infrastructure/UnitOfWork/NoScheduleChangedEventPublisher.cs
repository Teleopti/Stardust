using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NoScheduleChangedEventPublisher : ScheduleChangedEventPublisher
	{
		public NoScheduleChangedEventPublisher(IEventPopulatingPublisher publisher) : base(publisher)
		{
		}

		public override void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
		}
	}
}