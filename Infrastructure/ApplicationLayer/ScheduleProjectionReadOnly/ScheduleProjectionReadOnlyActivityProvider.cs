using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly
{
	public class ScheduleProjectionReadOnlyActivityProvider : IScheduleProjectionReadOnlyActivityProvider
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleProjectionReadOnlyActivityProvider(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public List<ISiteActivity> GetActivitiesBySite (ISite site, DateTimePeriod period, IScenario scenario,
			bool onlyShowActivitiesRequiringSeats)
		{
			const string query = @"    SELECT scheduleProjection.PersonId, activity.Id ActivityId, site.Id SiteId, scheduleProjection.StartDateTime, scheduleProjection.EndDateTime, activity.RequiresSeat
	                                        from Activity activity
	                                        inner join ReadModel.ScheduleProjectionReadOnly scheduleProjection 
	                                        on activity.Id = scheduleProjection.PayloadId
                                            and (:onlyShowActivitiesRequiringSeats = 0 or activity.RequiresSeat = 1)
	                                        inner join Person p
	                                        on scheduleProjection.PersonId = p.Id
	                                        inner join PersonPeriod pp
                                            on pp.Parent = p.Id and pp.StartDate <= scheduleProjection.StartDateTime and pp.EndDate >= scheduleProjection.EndDateTime
											inner join Team team 
											on pp.Team = team.Id
											inner join Site site
											on team.Site = site.Id
											and team.Site = :siteId 
											where ScenarioId = :scenarioId
											and scheduleProjection.StartDateTime < :endDateTime and scheduleProjection.EndDateTime > :startDateTime
											order by scheduleProjection.StartDateTime";

			var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(query)
				.SetBoolean("onlyShowActivitiesRequiringSeats", onlyShowActivitiesRequiringSeats)
				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetGuid("siteId", site.Id.GetValueOrDefault())
				.SetGuid("scenarioId", scenario.Id.GetValueOrDefault())

				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteActivity)))
				.List<ISiteActivity>();
			return queryResult.ToList();
		}
	}


	

}