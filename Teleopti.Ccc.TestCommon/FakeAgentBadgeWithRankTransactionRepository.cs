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
			_agentBadgeWithRankTransactions.Remove(root);
		}

		public IAgentBadgeWithRankTransaction Get(Guid id)
		{
			return Load(id);
		}

		public IEnumerable<IAgentBadgeWithRankTransaction> LoadAll()
		{
			return _agentBadgeWithRankTransactions;
		}

		public IAgentBadgeWithRankTransaction Load(Guid id)
		{
			return _agentBadgeWithRankTransactions.FirstOrDefault(x => x.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; }
		public IAgentBadgeWithRankTransaction Find(IPerson person, int badgeType, DateOnly calculateDate)
		{
			return _agentBadgeWithRankTransactions
				.FirstOrDefault(x => x.Person.Id == person.Id && x.BadgeType == badgeType && x.CalculatedDate == calculateDate);
		}

		public IList<IAgentBadgeWithRankTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var personIds = personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray();
			return _agentBadgeWithRankTransactions.Where(x => personIds.Contains(x.Person.Id.GetValueOrDefault()) && period.Contains(x.CalculatedDate)).ToList();
		}

		public void ResetAgentBadges()
		{
			_agentBadgeWithRankTransactions.Clear();
		}
	}
}
