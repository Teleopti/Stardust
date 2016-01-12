using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobHelper : IJobHelper
	{
		private IRaptorRepository _repository;
		private ILogOnHelper _logOnHelper;
		private IMessageSender _messageSender;

		public JobHelper(ILoadAllTenants loadAllTenants, ITenantUnitOfWork tenantUnitOfWork, IAvailableBusinessUnitsProvider availableBusinessUnitsProvider)
		{
			_logOnHelper = new LogOnHelper(loadAllTenants, tenantUnitOfWork, availableBusinessUnitsProvider);
			var url = new MutableUrl();
			url.Configure(ConfigurationManager.AppSettings["MessageBroker"]);
			_messageSender = new HttpSender(new HttpClientM(new HttpServer(), url, new NewtonsoftJsonSerializer()));
		}

		public JobHelper(
			IRaptorRepository repository, 
			IMessageSender messageSender, 
			ILogOnHelper logOnHelper)
		{
			_repository = repository;
			_logOnHelper = logOnHelper;
			_messageSender = messageSender;
		}

		public IList<IBusinessUnit> BusinessUnitCollection
		{
			get { return _logOnHelper.GetBusinessUnitCollection(); }
		}

		public IEnumerable<TenantInfo> TenantCollection
		{
			get { return _logOnHelper.TenantCollection; }
		}

		public IRaptorRepository Repository
		{
			get { return _repository; }
		}
		
		public IMessageSender MessageSender
		{
			get { return _messageSender; }
		}

		public bool SelectDataSourceContainer(string dataSourceName)
		{
			return _logOnHelper.SelectDataSourceContainer(dataSourceName);
		}

		public IDataSource SelectedDataSource
		{
			get { return _logOnHelper.SelectedDataSourceContainer.DataSource; }
		}

		public void RefreshTenantList()
		{
			_logOnHelper.RefreshTenantList();
		}

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			if (!_logOnHelper.SetBusinessUnit(businessUnit))
			{
				return false;
			}

			//Create repository when logged in to raptor domain
			_repository = new RaptorRepository(
				_logOnHelper.SelectedDataSourceContainer.DataSource.Statistic.ConnectionString,
				ConfigurationManager.AppSettings["isolationLevel"]);

			return true;
		}

		public void LogOffTeleoptiCccDomain()
		{
			_repository = null;
		}
	}
}