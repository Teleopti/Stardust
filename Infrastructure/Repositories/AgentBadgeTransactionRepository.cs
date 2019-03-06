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
	public class AgentBadgeTransactionRepository : Repository<IAgentBadgeTransaction>, IAgentBadgeTransactionRepository
	{
		public static AgentBadgeTransactionRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentBadgeTransactionRepository(currentUnitOfWork, null, null);
		}

		public static AgentBadgeTransactionRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new AgentBadgeTransactionRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public AgentBadgeTransactionRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
		
		public IAgentBadgeTransaction Find(IPerson person, int badgeType, DateOnly calculateDate, bool isExternal)
		{
			InParameter.NotNull(nameof(person),person);
			InParameter.NotNull(nameof(badgeType), badgeType);
			InParameter.NotNull(nameof(calculateDate),calculateDate);

			var result = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", badgeType))
				.Add(Restrictions.Eq("CalculatedDate", calculateDate))
				.Add(Restrictions.Eq("IsExternal", isExternal))
				.UniqueResult<IAgentBadgeTransaction>();
			return result;
		}

		public IList<IAgentBadgeTransaction> Find(IPerson person, int badgeType, DateOnlyPeriod period, bool isExternal)
		{
			var result = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", badgeType))
				.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
				.Add(Restrictions.Eq("IsExternal", isExternal))
				.List<IAgentBadgeTransaction>();
			return result;
		}

		public IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var retList = new List<IAgentBadgeTransaction>();

			foreach (var personList in personCollection.Batch(400))
			{
				var result = Session.CreateCriteria(typeof (IAgentBadgeTransaction), "ab")
					.Add(Restrictions.InG("Person", personList.ToArray()))
					.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
					.List<IAgentBadgeTransaction>();

				retList.AddRange(result);
			}

			return retList;
		}

		public void Remove(DateOnlyPeriod period)
		{
			var existings = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "ab")
				.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
				.List<IAgentBadgeTransaction>();

			existings.ForEach(Remove);
		}

		public void ResetAgentBadges()
		{
			Session.CreateSQLQuery(@"EXEC dbo.ResetAgentBadges").ExecuteUpdate();
		}
	}
}