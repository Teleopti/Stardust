using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public ICollection<IAgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			var idList = personIdList as Guid[] ?? personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadge>();
			}

			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where BadgeType=:badgeType and Person in (:personIdList)";
			var result = Session.CreateSQLQuery(query)
				.SetParameterList("personIdList", idList)
				.SetInt16("badgeType", (Int16)badgeType)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
				.SetReadOnly(true)
				.List<IAgentBadge>();

			return result;
		}

		public ICollection<IAgentBadge> Find(IEnumerable<Guid> personIdList)
		{
			var idList = personIdList as Guid[] ?? personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadge>();
			}

			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person in (:personIdList)";
			var result = Session.CreateSQLQuery(query)
				.SetParameterList("personIdList", idList)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
				.SetReadOnly(true)
				.List<IAgentBadge>();

			return result;
		}

		public ICollection<IAgentBadge> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", (Guid)person.Id)
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
					.SetGuid("person", (Guid)person.Id)
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.UniqueResult<IAgentBadge>();
			return result;
		}

		public ICollection<IAgentBadge> GetAllAgentBadges()
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate from AgentBadge";
			
			var result = Session.CreateSQLQuery(query)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>();
			return result;
		}

		public ICollection<IAgentBadge> GetAllAgentBadges(BadgeType badgeType)
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate from AgentBadge where BadgeType=:badgeType";

			var result = Session.CreateSQLQuery(query)
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<IAgentBadge>();
			return result;
		}
	}
}