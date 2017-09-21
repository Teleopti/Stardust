using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class AbsenceRequestIntradayFilter : IAbsenceRequestIntradayFilter
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly INow _now;
		private readonly IRequestProcessor _requestProcessor;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;

		public AbsenceRequestIntradayFilter(IRequestProcessor requestProcessor,
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

			var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest) personRequest.Request);
			var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);


			if (checkIfNoValidatorIsUsed(validators))
			{
				//this looks strange but is how it works. Pending = no autogrant, Grant = autogrant
				var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);
				if (autoGrant)
				{
					var command = new ApproveRequestCommand
					{
						PersonRequestId = personRequest.Id.GetValueOrDefault(),
						IsAutoGrant = true
					};
					_commandDispatcher.Execute(command);
				}
			}
			else
			{
				var isIntradayRequest = personRequest.Request.Period.ElapsedTime() <= TimeSpan.FromDays(1) && intradayPeriod.Contains(personRequest.Request.Period.EndDateTime);
				if (isIntradayRequest && validators.Any(v => v is StaffingThresholdValidator))
				{
					_requestProcessor.Process(personRequest, startDateTime);
				}
				else
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

		private bool checkIfNoValidatorIsUsed(IEnumerable<IAbsenceRequestValidator> validators)
		{
			if (validators.Any(v => v is StaffingThresholdValidator) ||
				validators.Any(v => v is StaffingThresholdValidatorCascadingSkillsWithShrinkage) ||
				validators.Any(v => v is BudgetGroupAllowanceValidator) ||
				validators.Any(v => v is BudgetGroupHeadCountValidator) ||
				validators.Any(v => v is StaffingThresholdWithShrinkageValidator))
				return false;
			return true;
		}
	}
}
