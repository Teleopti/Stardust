namespace Teleopti.Wfm.Api
{
	public class HashWrapper
	{
		const string salt = "$2a$10$WbJpBWPWsrVOlMze27m0ne";

		public string Hash(string token)
		{
			return BCrypt.Net.BCrypt.HashPassword(token, salt);
		}
	}
}