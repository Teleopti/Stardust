using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class HashFromToken
	{
		public string Generate(string salt, string token)
		{
			return BCryptTokenHashFunction.CreateHash(salt, token);
		}
	}
}