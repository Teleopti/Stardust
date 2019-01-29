using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DenyRequestCommandHandler : IHandleCommand<DenyRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly ICurrentScenario _currentScenario;

		public DenyRequestCommandHandler(IPersonRequestRepository personRequestRepository,
			IPersonRequestCheckAuthorization authorization, ICurrentScenario currentScenario)
		{
			_personRequestRepository = personRequestRepository;
			_authorization = authorization;
			_currentScenario = currentScenario;
		}

		public void Handle(DenyRequestCommand command)
		{
			command.ErrorMessages = new List<string>();

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);

			if (personRequest != null && denyRequest(personRequest, command))
			{
				command.AffectedRequestId = command.PersonRequestId;
				command.IsReplySuccess = command.TryReplyMessage(personRequest);
			}
		}
		private bool denyRequest(IPersonRequest personRequest, DenyRequestCommand command)
		{
			try
			{
				if (_currentScenario.Current() != null && _currentScenario.Current().Restricted && !PrincipalAuthorization.Current_DONTUSE()
						.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
				{
					command.ErrorMessages.Add(Resources.CanNotApproveOrDenyRequestDueToNoPermissionToModifyRestrictedScenarios);
					return false;
				}

				var denyOption = !command.IsManualDeny ? PersonRequestDenyOption.AutoDeny : PersonRequestDenyOption.None;
				denyOption = denyOption | command.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None);
				personRequest.Deny(command.DenyReason, _authorization, null, denyOption);

				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				command.ErrorMessages.Add(string.Format(Resources.RequestInvalidStateTransition, personRequest.StatusText,
					Resources.RequestStatusDenied));
			}

			return false;
		}
	}
}
