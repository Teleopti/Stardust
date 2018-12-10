using System;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class AbsenceRequestProcessor : IAbsenceRequestProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly INow _now;
		private readonly IRequestProcessor _requestProcessor;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;

		public AbsenceRequestProcessor(IRequestProcessor requestProcessor,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, INow now,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, ICommandDispatcher commandDispatcher,
			IAbsenceRequestSetting absenceRequestSetting)
		{
			_requestProcessor = requestProcessor;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_now = now;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_commandDispatcher = commandDispatcher;
			_absenceRequestSetting = absenceRequestSetting;
		}


		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();
			var startDateTime = _now.UtcDateTime();
			var intradayPeriod = new DateTimePeriod(startDateTime, startDateTime.AddHours(_absenceRequestSetting.ImmediatePeriodInHours));

			var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest) personRequest.Request);

			if (!_absenceRequestValidatorProvider.IsAnyStaffingValidatorEnabled(mergedPeriod))
			{
				approveRequest(personRequest, mergedPeriod);
			}
			else if (isIntradayRequest(personRequest, intradayPeriod) && _absenceRequestValidatorProvider.IsValidatorEnabled<StaffingThresholdValidator>(mergedPeriod))
			{
				_requestProcessor.Process(personRequest);
			}
			else
			{
				enqueueAbsenceRequest(personRequest);
			}
		}

		private static bool isIntradayRequest(IPersonRequest personRequest, DateTimePeriod intradayPeriod)
		{
			return personRequest.Request.Period.ElapsedTime() <= TimeSpan.FromDays(1) && intradayPeriod.Contains(personRequest.Request.Period.EndDateTime);
		}

		private void approveRequest(IPersonRequest personRequest, IAbsenceRequestOpenPeriod mergedPeriod)
		{
			var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);
			if (!autoGrant) return;
			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);
		}

		private void enqueueAbsenceRequest(IPersonRequest personRequest)
		{
			var queuedAbsenceRequest = new QueuedAbsenceRequest
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
