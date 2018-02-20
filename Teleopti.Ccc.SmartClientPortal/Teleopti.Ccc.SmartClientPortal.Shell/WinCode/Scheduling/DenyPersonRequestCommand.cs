using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class DenyPersonRequestCommand : IHandlePersonRequestCommand
	{
		private readonly IRequestPresenterCallback _callback;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private const string denyReasonResourceKey = nameof(Resources.RequestDenyReasonSupervisor);

		public DenyPersonRequestCommand(IRequestPresenterCallback callback, IPersonRequestCheckAuthorization authorization)
		{
			_callback = callback;
			_authorization = authorization;
		}

		public void Execute()
		{
			Model.PersonRequest.Deny(denyReasonResourceKey,_authorization);
			_callback.CommitUndo();
		}

		public PersonRequestViewModel Model
		{
			get; set;
		}
	}
}