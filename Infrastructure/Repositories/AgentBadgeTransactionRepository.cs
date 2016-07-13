using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeTransactionRepository : Repository<IAgentBadgeTransaction>, IAgentBadgeTransactionRepository
	{
#pragma warning disable 618
		public AgentBadgeTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public AgentBadgeTransactionRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
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

		public IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var retList = new List<IAgentBadgeTransaction>();

			foreach (var personList in personCollection.Batch(400))
			{
				var result = Session.CreateCriteria(typeof (IAgentBadgeTransaction), "ab")
					.Add(Restrictions.In("Person", new List<IPerson>(personList)))
					.Add(Restrictions.Between("CalculatedDate", period.StartDate, period.EndDate))
					.List<IAgentBadgeTransaction>();

				retList.AddRange(result);
			}

			return retList;
		}


		public void ResetAgentBadges()
		{
			Session.CreateSQLQuery(@"EXEC dbo.ResetAgentBadges").ExecuteUpdate();
		}
	}
}