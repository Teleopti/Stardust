using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeRepository : IAgentBadgeRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public AgentBadgeRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where BadgeType=:badgeType and Person in (:personIdList)";
			
			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<AgentBadge>();
			}

			var result = new List<AgentBadge>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(_currentUnitOfWork.Current().Session().CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetEnum("badgeType", badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentBadge)))
					.SetReadOnly(true)
					.List<AgentBadge>());
			}
			
			return result;
		}

		public ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList)
		{
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person in (:personIdList)";

			var idList = personIdList.ToArray();
			if (!idList.Any())
			{
				return new List<AgentBadge>();
			}

			var result = new List<AgentBadge>();
			var batch = idList.Batch(1000);
			foreach (var batchOfPeopleId in batch)
			{
				result.AddRange(_currentUnitOfWork.Current().Session().CreateSQLQuery(query)
					.SetParameterList("personIdList", batchOfPeopleId.ToArray())
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.List<AgentBadge>());
			}

			return result;
		}
		
		public AgentBadge Find(IPerson person, BadgeType badgeType)
		{
			InParameter.NotNull("person", person);
			InParameter.NotNull("badgeType", badgeType);
			const string query = @"select Person, BadgeType, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person and BadgeType=:badgeType";
			var result = _currentUnitOfWork.Current().Session().CreateSQLQuery(query)
					.SetGuid("person", person.Id.GetValueOrDefault())
					.SetEnum("badgeType", badgeType)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.UniqueResult<AgentBadge>();
			return result;
		}
	}
}