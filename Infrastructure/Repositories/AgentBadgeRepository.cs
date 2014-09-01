using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeRepository : Repository<IAgentBadge>, IAgentBadgeRepository
	{
		public AgentBadgeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public AgentBadgeRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public AgentBadgeRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadge> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			ICollection<IAgentBadge> retList = Session.CreateCriteria(typeof(AgentBadge), "badge")
							.Add(Restrictions.Eq("Person", person))
							.List<IAgentBadge>();
			return retList;
		}

		public IAgentBadge Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);

			ICollection<IAgentBadge> retList = Session.CreateCriteria(typeof(AgentBadge), "badge")
							.Add(Restrictions.Eq("Person", person))
							.Add(Restrictions.Eq("BadgeType", (int)badgeType))
							.List<IAgentBadge>();
			return retList.Any() ? retList.First() : null;
		}
	}
}