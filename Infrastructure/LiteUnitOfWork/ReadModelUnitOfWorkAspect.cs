using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IMessageSender _messageSender;
		private readonly ICurrentIdentity _currentIdentity;

		public ReadModelUnitOfWorkAspect(ICurrentDataSource currentDataSource, IMessageSender messageSender, ICurrentIdentity currentIdentity)
		{
			_currentDataSource = currentDataSource;
			_messageSender = messageSender;
			_currentIdentity = currentIdentity;
		}

		public void OnBeforeInvokation()
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.StartUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.EndUnitOfWork(exception);
			if (exception == null)
			{
				_messageSender.Send(new Notification
				{
					DomainType = "ReadModelUpdatedMessage",
					BinaryData = JsonConvert.SerializeObject(new ReadModelUpdatedMessage()),
					DataSource = _currentDataSource.Current().DataSourceName,
					BusinessUnitId = _currentIdentity.Current().BusinessUnit.Id.GetValueOrDefault().ToString()
				});
			}
		}
	}

	public class ReadModelUpdatedMessage
	{
	}
}