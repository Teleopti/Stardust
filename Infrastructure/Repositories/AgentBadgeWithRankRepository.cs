using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeWithRankRepository : Repository<IAgentBadgeWithRank>, IAgentBadgeWithRankRepository
	{
		public AgentBadgeWithRankRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadgeWithRank>();
			}

			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where BadgeType=:badgeType and Person in (:personIdList)";
			
			var result = new List<IAgentBadgeWithRank>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(Session.CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetInt16("badgeType", (Int16) badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentBadgeWithRank)))
					.SetReadOnly(true)
					.List<IAgentBadgeWithRank>());
			}

			return result;
		}

		public ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList)
		{
			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<IAgentBadgeWithRank>();
			}

			const string query = @"select Person, BadgeType, BronzeBadgeAmount, SilverBadgeAmount, GoldBadgeAmount, LastCalculatedDate "
				+ "from AgentBadgeWithRank where Person in (:personIdList)";

			var result = new List<IAgentBadgeWithRank>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(Session.CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentBadgeWithRank)))
					.SetReadOnly(true)
					.List<IAgentBadgeWithRank>());
			}

			return result;
		}

		public ICollection<IAgentBadgeWithRank> Find(IPerson person)
		{
			InParameter.NotNull(nameof(person), person);

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
			InParameter.NotNull(nameof(person), person);
			InParameter.NotNull(nameof(badgeType), badgeType);
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
	}
}