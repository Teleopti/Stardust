using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Api.Test
{
	public class FakeTokenVerifier : ITokenVerifier
	{
		public static Guid UserId = new Guid("{A16223F1-CF2B-4815-A4F6-CF40B192C66E}");

		private readonly Dictionary<string, UserIdWithTenant> hashUsers =
			new Dictionary<string, UserIdWithTenant> {{ "afdsafasdf", new UserIdWithTenant {Tenant = "asdf", UserId = UserId}}};

		public bool TryGetUser(string token, out UserIdWithTenant user)
		{
			return hashUsers.TryGetValue(token, out user);
		}
	}
}