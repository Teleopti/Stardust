using System;
using System.Globalization;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IStardustSender
	{
		Guid Send(IEvent @event);
	}

	public class StardustSender : IStardustSender
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(StardustSender));
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IConfigReader _configReader;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		
		public StardustSender(IPostHttpRequest postHttpRequest, ILoggedOnUser loggedOnUser, IConfigReader configReader, ICurrentDataSource currentDataSource, ICurrentBusinessUnit currentBusinessUnit)
		{
			_postHttpRequest = postHttpRequest;
			_loggedOnUser = loggedOnUser;
			_configReader = configReader;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public Guid Send(IEvent @event)
		{
			var type = @event.GetType().ToString();
			var job = @event as IStardustJobInfo;

			var userName = job?.UserName ?? _loggedOnUser.CurrentUserName() ?? "Stardust";
			var jobName = job?.JobName ?? type;
			var policy = job?.Policy ?? "";

			if (@event is ILogOnContext raptorDomainMessage)
			{
				var datasource = raptorDomainMessage.LogOnDatasource;
				if (string.IsNullOrEmpty(datasource))
					datasource = _currentDataSource.CurrentName();

				raptorDomainMessage.LogOnDatasource = datasource;
				if(raptorDomainMessage.LogOnBusinessUnitId == Guid.Empty)
					raptorDomainMessage.LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault();

				if (logger.IsDebugEnabled)
				{
					logger.Debug(string.Format(CultureInfo.InvariantCulture,
						"Sending {0} message (Data source = {1})",
						jobName, datasource));
				}
			}

			var ser = JsonConvert.SerializeObject(@event);
			var jobModel = new JobRequestModel
			{
				Name = jobName,
				Serialized = ser,
				Type = type,
				CreatedBy = userName,
				Policy = policy
			};
			var mess = JsonConvert.SerializeObject(jobModel);
			

			var managerLocation = _configReader.AppConfig("ManagerLocation");
			return _postHttpRequest.Send<Guid>(managerLocation + "job", mess);
		}
	}

	public class JobRequestModel
	{
		public string Name { get; set; }
		public string Serialized { get; set; }
		public string Type { get; set; }
		public string CreatedBy { get; set; }
		public string Policy { get; set; }
	}
}