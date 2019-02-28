using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentDayScheduleTagRepository : Repository<IAgentDayScheduleTag>, IAgentDayScheduleTagRepository
	{
		public static AgentDayScheduleTagRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentDayScheduleTagRepository(currentUnitOfWork, null, null);
		}

		public static AgentDayScheduleTagRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new AgentDayScheduleTagRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public AgentDayScheduleTagRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IList<IAgentDayScheduleTag> Find(DateTimePeriod period, IScenario scenario)
		{
			ICriteria crit = Session.CreateCriteria(typeof(AgentDayScheduleTag))
				.Add(Restrictions.Between("TagDate", period.StartDateTime, period.EndDateTime))
				.Add(Restrictions.Eq("Scenario", scenario));
			IList<IAgentDayScheduleTag> retList = crit.List<IAgentDayScheduleTag>();
			return retList;
		}

		public ICollection<IAgentDayScheduleTag> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			var retList = new List<IAgentDayScheduleTag>();
			foreach (var personList in personCollection.Batch(400))
			{
				ICriteria crit = Session.CreateCriteria(typeof(AgentDayScheduleTag))
				.Add(Restrictions.Between("TagDate", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
				.Add(Restrictions.InG("Person", personList))
				.Add(Restrictions.Eq("Scenario", scenario))
				.AddOrder(Order.Asc("Person"))
				.AddOrder(Order.Asc("TagDate"));
				retList.AddRange(crit.List<IAgentDayScheduleTag>());
			}

			return retList;
		}

		public IAgentDayScheduleTag LoadAggregate(Guid id)
		{
			return Get(id);
		}
	}
}
