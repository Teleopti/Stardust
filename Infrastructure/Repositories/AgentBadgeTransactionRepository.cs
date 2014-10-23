using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeTransactionRepository : Repository<IAgentBadgeTransaction>, IAgentBadgeTransactionRepository
	{
		public AgentBadgeTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public AgentBadgeTransactionRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public AgentBadgeTransactionRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadgeTransaction> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			var result = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.List<IAgentBadgeTransaction>();
			return result;
		}

		public ICollection<IAgentBadgeTransaction> Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);

			var result = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", (int)badgeType))
				.List<IAgentBadgeTransaction>();
			return result;
		}

		public IAgentBadgeTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			InParameter.NotNull("calculateDate", calculateDate);

			var result = Session.CreateCriteria(typeof(IAgentBadgeTransaction), "badge")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("BadgeType", (int)badgeType))
				.Add(Restrictions.Eq("CalculatedDate", calculateDate))
				.UniqueResult<IAgentBadgeTransaction>();
			return result;
		}

		public void ResetAgentBadges()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = ((NHibernateUnitOfWork) uow).Session;
				using (var tx = session.BeginTransaction())
				{
					//session.CreateSQLQuery("truncate table dbo.AgentBadgeTransaction").ExecuteUpdate();
					//tx.Commit();
					var deleteAll = string.Format("DELETE FROM dbo.AgentBadgeTransaction");
					session.CreateSQLQuery(deleteAll).ExecuteUpdate();
					tx.Commit();
				}
			}
		}
	}
}