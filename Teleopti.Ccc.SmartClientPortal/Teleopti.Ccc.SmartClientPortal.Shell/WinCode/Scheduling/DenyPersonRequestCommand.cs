using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class DenyPersonRequestCommand : IHandlePersonRequestCommand
	{
		private readonly IRequestPresenterCallback _callback;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private const string DenyReasonResourceKey = "RequestDenyReasonSupervisor";

		public DenyPersonRequestCommand(IRequestPresenterCallback callback, IPersonRequestCheckAuthorization authorization)
		{
			_callback = callback;
			_authorization = authorization;
		}

		public void Execute()
		{
			Model.PersonRequest.Deny(DenyReasonResourceKey,_authorization);
			_callback.CommitUndo();
		}

		public PersonRequestViewModel Model
		{
			get; set;
		}
	}
}