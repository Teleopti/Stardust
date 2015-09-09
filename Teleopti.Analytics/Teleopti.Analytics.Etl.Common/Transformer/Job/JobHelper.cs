using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.SignalR;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobHelper : IJobHelper
	{
		private IRaptorRepository _repository;
		private ILogOnHelper _logHelp;
		private ISignalRClient _messageClient;
		private IMessageSender _messageSender;

		public JobHelper(ILoadAllTenants loadAllTenants, ITenantUnitOfWork tenantUnitOfWork,
			IAvailableBusinessUnitsProvider availableBusinessUnitsProvider, IDataSourcesFactory dataSourcesFactory)
		{
			_logHelp = new LogOnHelper(loadAllTenants, tenantUnitOfWork, availableBusinessUnitsProvider, dataSourcesFactory);
			MessageBrokerContainerDontUse.Configure(
				ConfigurationManager.AppSettings["MessageBroker"],
				new IConnectionKeepAliveStrategy[] {},
				null,
				new NewtonsoftJsonSerializer(), new NewtonsoftJsonSerializer());
			_messageSender = MessageBrokerContainerDontUse.Sender();
			_messageClient = MessageBrokerContainerDontUse.SignalRClient();
		}

		public JobHelper(IRaptorRepository repository, ISignalRClient messageClient, IMessageSender messageSender, ILogOnHelper logOnHelper)
		{
			_repository = repository;
			_logHelp = logOnHelper;
			_messageClient = messageClient;
			_messageSender = messageSender;
		}

		public IList<IBusinessUnit> BusinessUnitCollection
		{
			get { return _logHelp.GetBusinessUnitCollection(); }
		}

		public List<ITenantName> TenantCollection
		{
			get { return _logHelp.TenantCollection; }
		}

		public IRaptorRepository Repository
		{
			get { return _repository; }
		}

		public ISignalRClient MessageClient
		{
			get { return _messageClient; }
		}

		public IMessageSender MessageSender
		{
			get { return _messageSender; }
		}

		public bool SelectDataSourceContainer(string dataSourceName)
		{
			return _logHelp.SelectDataSourceContainer(dataSourceName);
		}

		public IDataSource SelectedDataSource { get { return _logHelp.SelectedDataSourceContainer.DataSource; } }

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			if (!_logHelp.SetBusinessUnit(businessUnit))
			{
				return false;
			}

			//Create repository when logged in to raptor domain
			_repository = new RaptorRepository(_logHelp.SelectedDataSourceContainer.DataSource.Statistic.ConnectionString,
														  ConfigurationManager.AppSettings["isolationLevel"]);
			return true;
		}

		public void LogOffTeleoptiCccDomain()
		{
			_logHelp.LogOff();
			_repository = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		protected virtual void ReleaseManagedResources()
		{
			_repository = null;
			if (_logHelp != null)
				_logHelp.Dispose();
			_logHelp = null;
			_messageClient = null;
			_messageSender = null;
		}

	}
}