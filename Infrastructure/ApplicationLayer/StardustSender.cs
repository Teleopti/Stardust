using System;
using System.Globalization;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly IUpdatedBy _updatedBy;
		private readonly IConfigReader _configReader;

		public StardustSender(IPostHttpRequest postHttpRequest, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator, IUpdatedBy updatedBy, IConfigReader configReader)
		{
			_postHttpRequest = postHttpRequest;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_updatedBy = updatedBy;
			_configReader = configReader;
		}

		public Guid Send(IEvent @event)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);

			var userName = "Stardust";
			if (_updatedBy.Person() != null)
				userName = _updatedBy.Person().Name.ToString();
			else if(TeleoptiPrincipal.CurrentPrincipal != null && ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person != null)
				userName = ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.Name.ToString();

			var jobName = @event.GetType().ToString();
			var type = @event.GetType().ToString();
			var job = @event as IStardustJobInfo;
			var policy = "";
			if (job?.JobName != null)
			{
				jobName = job.JobName;
			}
			if (job?.UserName != null)
			{
				userName = job.UserName;
			}
			if (job?.Policy != null)
			{
				policy = job.Policy;
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
			if (logger.IsDebugEnabled)
			{
				var datasource = "<unknown>";
				var raptorDomainMessage = @event as ILogOnContext;
				if (raptorDomainMessage != null)
				{
					datasource = raptorDomainMessage.LogOnDatasource;
				}
				logger.Debug(string.Format(CultureInfo.InvariantCulture,
											"Sending {0} message (Data source = {1})",
											jobName, datasource));
			}

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