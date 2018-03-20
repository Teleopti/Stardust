using System.Collections.Generic;

namespace Teleopti.Wfm.Api
{
	public class TokenVerifier
	{
		readonly HashWrapper hash;
		public Dictionary<string, string> hashUsers = new Dictionary<string, string> { { "$2a$10$WbJpBWPWsrVOlMze27m0neJ0PLXBKVYS/y5tnMpuOY174/H1byvwC", "asdf" } };

		public TokenVerifier(HashWrapper hash)
		{
			this.hash = hash;
		}

		public bool TryGetUser(string token, out string user)
		{
			string v = hash.Hash(token);
			return hashUsers.TryGetValue(v, out user);
		}
	}
}