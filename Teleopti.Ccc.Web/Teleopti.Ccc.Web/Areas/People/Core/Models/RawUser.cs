namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class RawUser
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string ErrorMessage { get; set; }
	}
}