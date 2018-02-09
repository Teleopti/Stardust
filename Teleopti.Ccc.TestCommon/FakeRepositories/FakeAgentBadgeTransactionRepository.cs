using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentBadgeTransactionRepository :IAgentBadgeTransactionRepository
	{
		private IList<IAgentBadgeTransaction> _agentBadgeTransactions = new List<IAgentBadgeTransaction>();

		public void Add(IAgentBadgeTransaction root)
		{
			_agentBadgeTransactions.Add(root);
		}

		public void Remove(IAgentBadgeTransaction root)
		{
			_agentBadgeTransactions.Remove(root);
		}

		public IAgentBadgeTransaction Get(Guid id)
		{
			return Load(id);
		}

		public IEnumerable<IAgentBadgeTransaction> LoadAll()
		{
			return _agentBadgeTransactions;
		}

		public IAgentBadgeTransaction Load(Guid id)
		{
			return _agentBadgeTransactions.FirstOrDefault(x => x.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; }
		public IAgentBadgeTransaction Find(IPerson person, int badgeType, DateOnly calculateDate)
		{
			return _agentBadgeTransactions.FirstOrDefault(x =>
				x.Person.Id == person.Id && x.BadgeType == badgeType && x.CalculatedDate == calculateDate);
		}

		public IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var personIds = personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray();

			return _agentBadgeTransactions.Where(t => personIds.Contains(t.Person.Id.GetValueOrDefault()) && period.Contains(t.CalculatedDate)).ToList();
		}

		public void ResetAgentBadges()
		{
			_agentBadgeTransactions.Clear();
		}
	}
}
