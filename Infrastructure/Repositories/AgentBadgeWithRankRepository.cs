using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeWithRankRepository : Repository<IAgentBadgeWithRank>, IAgentBadgeWithRankRepository
	{
		public AgentBadgeWithRankRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public AgentBadgeWithRankRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public AgentBadgeWithRankRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			var idList = personIdList as Guid[] ?? personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadgeWithRank>();
			}

			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where BadgeType=:badgeType and Person in (:personIdList)";
			var result = Session.CreateSQLQuery(query)
				.SetParameterList("personIdList", idList)
				.SetInt16("badgeType", (Int16)badgeType)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadgeWithRank)))
				.SetReadOnly(true)
				.List<IAgentBadgeWithRank>();

			return result;
		}

		public ICollection<IAgentBadgeWithRank> Find(IPerson person)
		{
			InParameter.NotNull("person", person);

			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where Person = :person";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", (Guid)person.Id)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadgeWithRank)))
					.SetReadOnly(true)
					.List<IAgentBadgeWithRank>();
			return result;
		}

		public IAgentBadgeWithRank Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where Person = :person and BadgeType=:badgeType";
			var result = Session.CreateSQLQuery(query)
					.SetGuid("person", (Guid)person.Id)
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadgeWithRank)))
					.SetReadOnly(true)
					.UniqueResult<IAgentBadgeWithRank>();
			return result;
		}

		public ICollection<IAgentBadgeWithRank> GetAllAgentBadges()
		{
			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank";
			
			var result = Session.CreateSQLQuery(query)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadgeWithRank)))
					.SetReadOnly(true)
					.List<IAgentBadgeWithRank>();
			return result;
		}

		public ICollection<IAgentBadgeWithRank> GetAllAgentBadges(BadgeType badgeType)
		{
			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where BadgeType=:badgeType";

			var result = Session.CreateSQLQuery(query)
					.SetInt16("badgeType", (Int16)badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadgeWithRank)))
					.SetReadOnly(true)
					.List<IAgentBadgeWithRank>();
			return result;
		}
	}
}