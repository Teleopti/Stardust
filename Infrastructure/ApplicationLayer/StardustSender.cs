using System;
using System.Configuration;
using System.Globalization;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IStardustSender
	{
		Guid Send(EventWithInfrastructureContext @event, string jobName, string userName, string type);
	}

	public class StardustSender : IStardustSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StardustSender));
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

		public StardustSender(IPostHttpRequest postHttpRequest, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
		{
			_postHttpRequest = postHttpRequest;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
		}

		public Guid Send(EventWithInfrastructureContext @event, string jobName, string userName, string type)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);
			var ser = JsonConvert.SerializeObject(@event);
			var jobModel = new JobRequestModel
			{
				Name = jobName,
				Serialized = ser,
				Type = type,
				UserName = userName
			};
			var mess = JsonConvert.SerializeObject(jobModel);
			if (Logger.IsDebugEnabled)
			{
				var datasource = "<unknown>";
				var raptorDomainMessage = @event as ILogOnContext;
				if (raptorDomainMessage != null)
				{
					datasource = raptorDomainMessage.LogOnDatasource;
				}
				Logger.Debug(string.Format(CultureInfo.InvariantCulture,
											"Sending {0} message (Data source = {1})",
											jobName, datasource));
			}

			return _postHttpRequest.Send<Guid>(ConfigurationManager.AppSettings["ManagerLocation"] + "job", mess);
		}
	}

	internal class JobRequestModel
	{
		public string Name { get; set; }
		public string Serialized { get; set; }
		public string Type { get; set; }
		public string UserName { get; set; }
	}
}