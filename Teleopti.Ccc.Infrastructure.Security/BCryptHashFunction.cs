namespace Teleopti.Ccc.Infrastructure.Security
{
	public class BCryptHashFunction : IHashFunction
	{
		private const int workFactor = 10;

		public string CreateHash(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
		}

		public bool Verify(string password, string hash)
		{
			return !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(hash) &&
				   BCrypt.Net.BCrypt.Verify(password, hash);
		}

		public bool IsGeneratedByThisFunction(string hash)
		{
			return hash == null || hash.StartsWith("$2a$") && hash.Length == 60;
		}
	}
}