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
			throw new NotImplementedException();
		}

		public IAgentBadgeTransaction Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAgentBadgeTransaction> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IAgentBadgeTransaction Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IAgentBadgeTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate)
		{
			throw new NotImplementedException();
		}

		public IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			var personIds = personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray();

			return _agentBadgeTransactions.Where(t => personIds.Contains(t.Person.Id.GetValueOrDefault()) && period.Contains(t.CalculatedDate)).ToList();
		}

		public void ResetAgentBadges()
		{
			throw new NotImplementedException();
		}
	}
}
