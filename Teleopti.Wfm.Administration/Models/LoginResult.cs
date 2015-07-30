namespace Teleopti.Wfm.Administration.Models
{
	public class LoginResult
	{
		public int Id { get; set; }
		public string  AccessToken { get; set; }
		public string UserName { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}