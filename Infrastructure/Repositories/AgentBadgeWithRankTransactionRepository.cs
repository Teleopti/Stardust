using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeWithRankTransactionRepository : Repository<IAgentBadgeWithRankTransaction>, IAgentBadgeWithRankTransactionRepository
	{
		public AgentBadgeWithRankTransactionRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public AgentBadgeWithRankTransactionRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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