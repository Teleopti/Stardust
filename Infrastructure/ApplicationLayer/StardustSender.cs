using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IStardustSender
	{
		Guid Send(IEvent @event);
	}

	public class StardustSender : IStardustSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StardustSender));
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly IUpdatedBy _updatedBy;

		public StardustSender(IPostHttpRequest postHttpRequest, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator, IUpdatedBy updatedBy)
		{
			_postHttpRequest = postHttpRequest;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_updatedBy = updatedBy;
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
			if (job != null && job.JobName != null)
			{
				jobName = job.JobName;
			}
			if (job != null && job.UserName != null)
			{
				userName = job.UserName;
			}
			var ser = JsonConvert.SerializeObject(@event);
			var jobModel = new JobRequestModel
			{
				Name = jobName,
				Serialized = ser,
				Type = type,
				CreatedBy = userName
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

	public class JobRequestModel
	{
		public string Name { get; set; }
		public string Serialized { get; set; }
		public string Type { get; set; }
		public string CreatedBy { get; set; }
	}
}