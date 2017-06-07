namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class PasswordWarningViewModel
	{
		public bool WillExpireSoon { get; set; }
		public bool AlreadyExpired { get; set; }
		public bool Failed { get; set; }
		public string Message { get; set; }
	}
}