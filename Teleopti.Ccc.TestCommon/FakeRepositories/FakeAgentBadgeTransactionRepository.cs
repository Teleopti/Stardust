using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentBadgeTransactionRepository :IAgentBadgeTransactionRepository
	{
		private readonly IList<IAgentBadgeTransaction> _agentBadgeTransactions = new List<IAgentBadgeTransaction>();
		private int _findByPersonListWasCalledTimes;

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
		public IAgentBadgeTransaction Find(IPerson person, int badgeType, DateOnly calculateDate, bool isExternal)
		{
			return _agentBadgeTransactions.FirstOrDefault(x =>
				x.Person.Id == person.Id && x.BadgeType == badgeType && x.CalculatedDate == calculateDate && x.IsExternal == isExternal);
		}

		public IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period)
		{
			_findByPersonListWasCalledTimes++;
			var personIds = personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray();

			return _agentBadgeTransactions.Where(t => personIds.Contains(t.Person.Id.GetValueOrDefault()) && period.Contains(t.CalculatedDate)).ToList();
		}

		public IList<IAgentBadgeTransaction> Find(IPerson person, int badgeType, DateOnlyPeriod period, bool isExternal)
		{
			return _agentBadgeTransactions.Where(x => x.Person.Id == person.Id 
													&& x.BadgeType == badgeType 
													&& period.Contains(x.CalculatedDate) 
													&& x.IsExternal == isExternal).ToList();
		}

		public void Remove(DateOnlyPeriod period)
		{
			var existings = _agentBadgeTransactions.Where(x => period.Contains(x.CalculatedDate)).ToList();
			existings.ForEach(Remove);
		}

		public void ResetAgentBadges()
		{
			_agentBadgeTransactions.Clear();
		}

		public int FindByPersonListCalledTimes()
		{
			return _findByPersonListWasCalledTimes;
		}
	}
}
