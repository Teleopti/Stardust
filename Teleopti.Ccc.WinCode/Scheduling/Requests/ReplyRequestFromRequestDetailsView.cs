namespace Teleopti.Ccc.WinCode.Scheduling.Requests
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