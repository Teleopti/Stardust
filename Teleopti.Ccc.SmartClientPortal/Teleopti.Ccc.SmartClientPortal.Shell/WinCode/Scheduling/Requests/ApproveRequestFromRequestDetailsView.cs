namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public class ApproveRequestFromRequestDetailsView
	{
		public PersonRequestViewModel Request { get; private set; }

		public ApproveRequestFromRequestDetailsView(PersonRequestViewModel request)
		{
			Request = request;
		}
	}
}