using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public class ControllableReadModelTransactionSyncronization : IReadModelTransactionSyncronization
	{
		private readonly IList<Action> _actionsWhenDone = new List<Action>();

		public void OnSuccessfulTransaction(Action done)
		{
			_actionsWhenDone.Add(done);
		}

		public void RunNow()
		{
			_actionsWhenDone.ForEach(a => a.Invoke());
		}
	}
}