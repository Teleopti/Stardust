using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAgentBadgeWithRankTransactionRepository :IAgentBadgeWithRankTransactionRepository
	{

		private IList<IAgentBadgeWithRankTransaction> _agentBadgeWithRankTransactions = new List<IAgentBadgeWithRankTransaction>();

		public void Add(IAgentBadgeWithRankTransaction root)
		{
			_agentBadgeWithRankTransactions.Add(root);
		}

		public void Remove(IAgentBadgeWithRankTransaction root)
		{
			throw new NotImplementedException();
		}

		public IAgentBadgeWithRankTransaction Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAgentBadgeWithRankTransaction> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IAgentBadgeWithRankTransaction Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IAgentBadgeWithRankTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate)
		{
			throw new NotImplementedException();
		}

		public IList<IAgentBadgeWithRankTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var personIds = personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray();
			return _agentBadgeWithRankTransactions.Where(x => personIds.Contains(x.Person.Id.GetValueOrDefault()) && period.Contains(x.CalculatedDate)).ToList();
		}

		public void ResetAgentBadges()
		{
			throw new NotImplementedException();
		}
	}
}
