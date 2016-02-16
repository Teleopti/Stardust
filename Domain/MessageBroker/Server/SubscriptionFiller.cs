using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class SubscriptionFiller : IBeforeSubscribe
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnitId;

		public SubscriptionFiller(ICurrentDataSource currentDataSource, ICurrentBusinessUnit currentBusinessUnitId)
		{
			_currentDataSource = currentDataSource;
			_currentBusinessUnitId = currentBusinessUnitId;
		}

		public void Invoke(Subscription subscription)
		{
			if (String.IsNullOrEmpty(subscription.DataSource))
			{
				subscription.DataSource = _currentDataSource.CurrentName();
			}
			if (String.IsNullOrEmpty(subscription.BusinessUnitId) || subscription.BusinessUnitId == Guid.Empty.ToString())
			{
				if (_currentBusinessUnitId.Current() != null)
					subscription.BusinessUnitId = _currentBusinessUnitId.Current().Id.ToString();
			}
		}
	}
}