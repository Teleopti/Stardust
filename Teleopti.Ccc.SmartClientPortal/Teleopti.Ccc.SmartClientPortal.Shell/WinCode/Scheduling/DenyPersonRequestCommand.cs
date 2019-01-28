using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class DenyPersonRequestCommand : IHandlePersonRequestCommand
	{
		private readonly IRequestPresenterCallback _callback;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IScenario _scenario;
		private readonly IViewBase _viewBase;

		private const string denyReasonResourceKey = nameof(Resources.RequestDenyReasonSupervisor);

		public DenyPersonRequestCommand(IRequestPresenterCallback callback, IPersonRequestCheckAuthorization authorization, IScenario scenario, IViewBase viewBase = null)
		{
			_callback = callback;
			_authorization = authorization;
			_scenario = scenario;
			_viewBase = viewBase;
		}

		public void Execute()
		{
			if (_scenario.Restricted && !PrincipalAuthorization.Current_DONTUSE()
					.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
			{
				_viewBase?.ShowErrorMessage(Resources.CanNotApproveOrDenyRequestDueToNoPermissionToModifyRestrictedScenarios, Resources.ViolationOfABusinessRule);

				_callback.CommitUndo();
				return;
			}
			Model.PersonRequest.Deny(denyReasonResourceKey,_authorization);
			_callback.CommitUndo();
		}

		public PersonRequestViewModel Model
		{
			get; set;
		}
	}
}