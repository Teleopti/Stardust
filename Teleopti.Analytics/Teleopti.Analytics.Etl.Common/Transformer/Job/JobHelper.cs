using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Foundation;
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

		public JobHelper(LogOnHelper logOnHelper)
		{
			_logOnHelper = logOnHelper;
			var url = new MutableUrl();
			url.Configure(ConfigurationManager.AppSettings["MessageBroker"]);
			_messageSender = new HttpSender(new HttpClientM(new HttpServer(), url, new NewtonsoftJsonSerializer()));
		}

		protected JobHelper(
			IRaptorRepository repository, 
			IMessageSender messageSender, 
			ILogOnHelper logOnHelper)
		{
			_repository = repository;
			_logOnHelper = logOnHelper;
			_messageSender = messageSender;
		}


		public IRaptorRepository Repository
		{
			get { return _repository; }
		}

		public IMessageSender MessageSender
		{
			get { return _messageSender; }
		}

		public void LogOffTeleoptiCccDomain()
		{
			_repository = null;
		}

		public IList<IBusinessUnit> BusinessUnitCollection
		{
			get { return _logOnHelper.GetBusinessUnitCollection(); }
		}

		public bool SelectDataSourceContainer(string dataSourceName)
		{
			return _logOnHelper.SelectDataSourceContainer(dataSourceName);
		}

		public IDataSource SelectedDataSource
		{
			get { return _logOnHelper.SelectedDataSourceContainer.DataSource; }
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

	}
}