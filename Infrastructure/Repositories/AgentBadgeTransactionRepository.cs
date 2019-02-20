using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeTransactionRepository : Repository<IAgentBadgeTransaction>, IAgentBadgeTransactionRepository
	{
#pragma warning disable 618
		public AgentBadgeTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public AgentBadgeTransactionRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
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