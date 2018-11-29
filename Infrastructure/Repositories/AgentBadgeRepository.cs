using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeRepository : IAgentBadgeRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public AgentBadgeRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList, int badgeType)
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
					.SetInt32("badgeType", badgeType)
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
		
		public AgentBadge Find(IPerson person, int badgeType, bool isExternal)
		{
			InParameter.NotNull(nameof(person), person);
			InParameter.NotNull(nameof(badgeType), badgeType);
			const string query = @"select Person, BadgeType, IsExternal, TotalAmount, LastCalculatedDate "
				+ "from AgentBadge where Person = :person and BadgeType=:badgeType and IsExternal=:isExternal";
			var result = _currentUnitOfWork.Current().Session().CreateSQLQuery(query)
					.SetGuid("person", person.Id.GetValueOrDefault())
					.SetInt32("badgeType", badgeType)
					.SetBoolean("isExternal", isExternal)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
					.SetReadOnly(true)
					.UniqueResult<AgentBadge>();
			return result;
		}

		public AgentBadge Find(IPerson person, int badgeType, bool isExternal, DateOnlyPeriod period)
		{
			InParameter.NotNull(nameof(person), person);
			InParameter.NotNull(nameof(badgeType), badgeType);
			const string query = @"select Person, BadgeType, IsExternal, TotalAmount, LastCalculatedDate "
								 + "from AgentBadge where Person = :person and BadgeType=:badgeType and IsExternal=:isExternal and LastCalculatedDate between :startDate and :endDate";
			var result = _currentUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetGuid("person", person.Id.GetValueOrDefault())
				.SetInt32("badgeType", badgeType)
				.SetBoolean("isExternal", isExternal)
				.SetDateOnly("startDate", period.StartDate)
				.SetDateOnly("endDate", period.EndDate)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentBadge)))
				.SetReadOnly(true)
				.UniqueResult<AgentBadge>();
			return result;
		}
	}
}