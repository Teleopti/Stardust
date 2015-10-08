using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeWithRankTransactionRepository : Repository<IAgentBadgeWithRankTransaction>, IAgentBadgeWithRankTransactionRepository
	{
		public AgentBadgeWithRankTransactionRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public AgentBadgeWithRankTransactionRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadgeWithRankTransaction> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			var result = Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.List<IAgentBadgeWithRankTransaction>();
			return result;
		}

		public ICollection<IAgentBadgeWithRankTransaction> Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);

			var result = Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", (int)badgeType))
				.List<IAgentBadgeWithRankTransaction>();
			return result;
		}

		public IAgentBadgeWithRankTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			InParameter.NotNull("calculateDate", calculateDate);

			var result = Session.CreateCriteria(typeof(IAgentBadgeWithRankTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", (int)badgeType))
				.Add(Restrictions.Eq("CalculatedDate", calculateDate))
				.UniqueResult<IAgentBadgeWithRankTransaction>();
			return result;
		}

		public void ResetAgentBadges()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = ((NHibernateUnitOfWork)uow).Session;
				using (var tx = session.BeginTransaction())
				{
					session.CreateSQLQuery(@"EXEC dbo.ResetAgentBadgesWithRank").ExecuteUpdate();
					tx.Commit();
				}
			}
		}
	}
}