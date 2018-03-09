namespace Teleopti.Ccc.Infrastructure.Security
{
	public static class BCryptTokenHashFunction
	{
		public static string CreateHash(string salt, string token)
		{
			return BCrypt.Net.BCrypt.HashPassword(token, salt);
		}

		public static string GenerateSalt()
		{
			return BCrypt.Net.BCrypt.GenerateSalt(8);
		}
	}
}