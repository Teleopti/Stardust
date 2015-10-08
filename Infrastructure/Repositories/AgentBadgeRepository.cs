using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

#pragma warning disable 618
		public AgentBadgeRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
#pragma warning restore 618
		{
		}

		public AgentBadgeRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where BadgeType=:badgeType and Person in (:personIdList)";
			
			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadge>();
			}

			var result = new List<IAgentBadge>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(Session.CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetInt16("badgeType", (Int16) badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>());
			}
			
			return result;
		}

		public ICollection<IAgentBadge> Find(IEnumerable<Guid> personIdList)
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person in (:personIdList)";

			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadge>();
			}

			var result = new List<IAgentBadge>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(Session.CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>());
			}

			return result;
		}

		public ICollection<IAgentBadge> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", person.Id.GetValueOrDefault())
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>();
			return result;
		}

		public IAgentBadge Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person and BadgeType=:badgeType";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", person.Id.GetValueOrDefault())
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.UniqueResult<IAgentBadge>();
			return result;
		}
	}
}