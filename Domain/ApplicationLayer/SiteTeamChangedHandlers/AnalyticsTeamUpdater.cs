using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers
{
	
	public class AnalyticsTeamUpdater : IHandleEvent<TeamNameChangedEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

		public AnalyticsTeamUpdater(IAnalyticsTeamRepository analyticsTeamRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			_analyticsTeamRepository = analyticsTeamRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(TeamNameChangedEvent @event)
		{
			_analyticsTeamRepository.UpdateName(@event.TeamId, @event.Name);
			_analyticsPersonPeriodRepository.UpdateTeamName(@event.TeamId, @event.Name);
		}
	}
}
