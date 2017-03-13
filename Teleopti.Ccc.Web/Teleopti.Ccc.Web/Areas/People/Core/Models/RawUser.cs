namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class RawUser
	{
		[Order(0)]
		public string Firstname { get; set; }
		[Order(1)]
		public string Lastname { get; set; }
		[Order(2)]
		public string WindowsUser { get; set; }
		[Order(3)]
		public string ApplicationUserId { get; set; }
		[Order(4)]
		public string Password { get; set; }
		[Order(5)]
		public string Role { get; set; }
		[Order(16)]
		public string ErrorMessage { get; set; }
	}
}