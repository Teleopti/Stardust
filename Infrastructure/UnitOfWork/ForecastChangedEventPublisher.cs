using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ForecastChangedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ForecastChangedEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var roots = modifiedRoots.Select(r => r.Root);
			var skillDays = roots.OfType<ISkillDay>().ToList();

			if (!skillDays.Any())
			{
				return;
			}

			var skillDayIds = new List<Guid>();

			foreach (var skillDay in skillDays)
			{
				if (skillDay is IAggregateRoot_Events events)
				{
					events.PopAllEvents(null);
				}
				skillDayIds.Add(skillDay.Id.Value);
			}

			_eventPublisher.Publish(new ForecastChangedEvent
			{
				SkillDayIds = skillDayIds.ToArray()
			});
		}
	}
}