using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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

			const string query = @"select BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", (Guid)person.Id)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>();
			foreach (var agentBadge in result)
			{
				agentBadge.Person = person;
			}
			return result;
		}

		public IAgentBadge Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			const string query = @"select BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person and BadgeType=:badgeType";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", (Guid)person.Id)
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.UniqueResult<IAgentBadge>();
			result.Person = person;
			return result;
		}
	}
}