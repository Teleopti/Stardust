﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.CommandProvider
{
	public class CancelAbsenceRequestCommandProvider : ICancelAbsenceRequestCommandProvider
	{
		private readonly IHandleCommand<CancelAbsenceRequestCommand> _cancelAbsenceRequestCommand;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public CancelAbsenceRequestCommandProvider(IHandleCommand<CancelAbsenceRequestCommand> cancelAbsenceRequestCommand, IPersonRequestRepository personRequestRepository, IPermissionProvider permissionProvider, IUserTimeZone userTimeZone, INow now)
		{
			_cancelAbsenceRequestCommand = cancelAbsenceRequestCommand;
			_personRequestRepository = personRequestRepository;
			_permissionProvider = permissionProvider;
			_userTimeZone = userTimeZone;
			_now = now;
		}

		public CancelAbsenceRequestCommand CancelAbsenceRequest (Guid personRequestId)
		{
			var command = new CancelAbsenceRequestCommand
			{
				PersonRequestId = personRequestId,
				ErrorMessages = new List<string>()
			};

			var personRequest = _personRequestRepository.Get (personRequestId);
			if (personRequest == null)
			{
				return null;
			}


			var workflowControlSet = personRequest.Person.WorkflowControlSet;
			if (workflowControlSet != null)
			{
				var threshold = workflowControlSet.AbsenceRequestCancellationThreshold ?? 0;
				var minDate = new DateOnly(personRequest.Request.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).AddDays(-threshold));

				var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
					personRequest.Person.PermissionInformation.DefaultTimeZone()));
				if (today > minDate)
				{
					command.ErrorMessages.Add(string.Format(Resources.AbsenceRequestCancellationThresholdExceeded, threshold));
					return command;
				}

			}
			
			if (!hasPermission (DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, personRequest))
			{
				command.ErrorMessages.Add (Resources.InsufficientPermission);
				return command;
			}
			
			_cancelAbsenceRequestCommand.Handle (command);

			return command;
		}

		private bool hasPermission(string applicationFunctionPath, IPersonRequest personRequest)
		{
			return _permissionProvider.HasPersonPermission (applicationFunctionPath, new DateOnly(personRequest.RequestedDate), personRequest.Person);
		}
	}
}
