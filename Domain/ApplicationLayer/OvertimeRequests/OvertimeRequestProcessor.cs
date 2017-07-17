using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));
		private const int minimumApprovalThresholdTimeInMinutes = 15;

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher, INow now, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public void Process(IPersonRequest personRequest)
		{
			if (isRequestStartTimeWithinUpcoming15Mins(personRequest))
			{
				var denyCommand = new DenyRequestCommand()
				{
					PersonRequestId = personRequest.Id.GetValueOrDefault(),
					DenyReason = string.Format(Resources.OvertimeRequestDenyReasonExpired, TimeZoneHelper.ConvertFromUtc(personRequest.Request.Period.StartDateTime,_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()), minimumApprovalThresholdTimeInMinutes),
					DenyOption = PersonRequestDenyOption.AutoDeny
				};
				_commandDispatcher.Execute(denyCommand);
				return;
			}

			personRequest.Pending();

			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private bool isRequestStartTimeWithinUpcoming15Mins(IPersonRequest personRequest)
		{
			var span = personRequest.Request.Period.StartDateTime - _now.UtcDateTime();
			if (Math.Ceiling(span.TotalMinutes) < minimumApprovalThresholdTimeInMinutes) return true;
			
			return false;
		}
	}
}