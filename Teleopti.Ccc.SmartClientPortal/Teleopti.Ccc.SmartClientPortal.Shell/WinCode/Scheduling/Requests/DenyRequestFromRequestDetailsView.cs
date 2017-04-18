namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public class DenyRequestFromRequestDetailsView
	{
		public PersonRequestViewModel Request { get; private set; }

		public DenyRequestFromRequestDetailsView(PersonRequestViewModel request)
		{
			Request = request;
		}
	}
}