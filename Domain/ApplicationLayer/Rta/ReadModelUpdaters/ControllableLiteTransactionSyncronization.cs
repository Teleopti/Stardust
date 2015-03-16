using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class ControllableLiteTransactionSyncronization : ILiteTransactionSyncronization
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