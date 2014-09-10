namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
	public class ReplyAndApproveRequestFromRequestDetailsView
	{
		public PersonRequestViewModel Request { get; private set; }

		public ReplyAndApproveRequestFromRequestDetailsView(PersonRequestViewModel  personRequestViewModel)
		{
			Request = personRequestViewModel;
		}
	}
}