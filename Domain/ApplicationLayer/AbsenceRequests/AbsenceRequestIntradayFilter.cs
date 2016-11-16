using System;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class AbsenceRequestIntradayFilter : IAbsenceRequestIntradayFilter 
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AbsenceRequestIntradayFilter));

		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IConfigReader _configReader;
		private readonly IIntradayRequestProcessor _intradayRequestProcessor;

		public AbsenceRequestIntradayFilter(IConfigReader configReader, 
			IIntradayRequestProcessor intradayRequestProcessor, 
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository)
		{
			_configReader = configReader;
			_intradayRequestProcessor = intradayRequestProcessor;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
		}

		
		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();
			var startDateTime = DateTime.UtcNow;

			var fakeIntradayStartUtcDateTime = _configReader.AppConfig("FakeIntradayUtcStartDateTime");
			if (fakeIntradayStartUtcDateTime != null)
			{
				try
				{
					startDateTime = DateTime.ParseExact(fakeIntradayStartUtcDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Utc();
				}
				catch
				{
					logger.Warn("The app setting 'FakeIntradayStartDateTime' is not specified correctly. Format your datetime as 'yyyy-MM-dd HH:mm' ");
				}
			}

			var intradayPeriod = new DateTimePeriod(startDateTime, startDateTime.AddHours(24));
			
			var validators = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest)personRequest.Request)
				.GetSelectedValidatorList();

			var isIntradayRequest = intradayPeriod.Contains(personRequest.Request.Period.StartDateTime) && intradayPeriod.Contains(personRequest.Request.Period.EndDateTime);
			if (isIntradayRequest && validators.Any(v => v is StaffingThresholdValidator))
			{
				_intradayRequestProcessor.Process(personRequest, startDateTime);
			}
			else
			{
				var queuedAbsenceRequest = new QueuedAbsenceRequest()
				{
					PersonRequest = personRequest.Id.GetValueOrDefault(),
					Created = personRequest.CreatedOn.GetValueOrDefault(),
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime
				};
				_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
			}
		}
	}
}
