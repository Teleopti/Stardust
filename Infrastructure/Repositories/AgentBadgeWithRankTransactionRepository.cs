using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeWithRankTransactionRepository : Repository<IAgentBadgeWithRankTransaction>, IAgentBadgeWithRankTransactionRepository
	{
		public static AgentBadgeWithRankTransactionRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentBadgeWithRankTransactionRepository(currentUnitOfWork, null, null);
		}

		public static AgentBadgeWithRankTransactionRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new AgentBadgeWithRankTransactionRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public AgentBadgeWithRankTransactionRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
		
		public IAgentBadgeWithRankTransaction Find(IPerson person, int badgeType, DateOnly calculateDate, bool isExternal)
		{
			InParameter.NotNull(nameof(person), person);

			var result = Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", badgeType))
				.Add(Restrictions.Eq("CalculatedDate", calculateDate))
				.Add(Restrictions.Eq("IsExternal", isExternal))
				.UniqueResult<IAgentBadgeWithRankTransaction>();
			return result;
		}

		public void Remove(DateOnlyPeriod period)
		{
			var existings =  Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "ab")
				.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
				.List<IAgentBadgeWithRankTransaction>();

			existings.ForEach(Remove);
		}

		public IList<IAgentBadgeWithRankTransaction> Find(IPerson person, int badgeType, DateOnlyPeriod period, bool isExternal)
		{
			return Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "ab")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", badgeType))
				.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
				.Add(Restrictions.Eq("IsExternal", isExternal))
				.List<IAgentBadgeWithRankTransaction>();
		}

		public IList<IAgentBadgeWithRankTransaction> Find(IEnumerable<IPerson> personCollection,DateOnlyPeriod period)
		{
			var retList = new List<IAgentBadgeWithRankTransaction>();

			foreach(var personList in personCollection.Batch(400))
			{
				var result = Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction),"ab")
					.Add(Restrictions.InG("Person", personList.ToArray()))
					.Add(Restrictions.Between("CalculatedDate",period.StartDate,period.EndDate))
					.List<IAgentBadgeWithRankTransaction>();

				retList.AddRange(result);
			}

			return retList;
		}

		public void ResetAgentBadges()
		{
			Session.CreateSQLQuery(@"EXEC dbo.ResetAgentBadgesWithRank").ExecuteUpdate();
		}
	}
}