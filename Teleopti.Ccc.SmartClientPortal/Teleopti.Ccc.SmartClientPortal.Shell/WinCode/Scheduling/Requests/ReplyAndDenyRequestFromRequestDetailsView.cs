namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
	public class ReplyAndDenyRequestFromRequestDetailsView
	{
		public PersonRequestViewModel Request { get; private set; }

		public ReplyAndDenyRequestFromRequestDetailsView(PersonRequestViewModel personRequestViewModel)
		{
			Request = personRequestViewModel;
		}
	}
}