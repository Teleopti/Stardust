namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public class ReplyRequestFromRequestDetailsView
	{
		public PersonRequestViewModel Request { get; private set; }

		public ReplyRequestFromRequestDetailsView(PersonRequestViewModel request)
		{
			Request = request;
		}
	}
}