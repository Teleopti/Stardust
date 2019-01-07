using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test
{
	public class FakeTokenVerifier : ITokenVerifier
	{
		public static Guid UserId = new Guid("{A16223F1-CF2B-4815-A4F6-CF40B192C66E}");

		private readonly Dictionary<string, UserIdWithTenant> hashUsers =
			new Dictionary<string, UserIdWithTenant> {{ "afdsafasdf", new UserIdWithTenant {Tenant = DomainTestAttribute.DefaultTenantName, UserId = UserId}}};

		public Task<ValueTuple<bool,UserIdWithTenant>> TryGetUserAsync(string token)
		{
			var result = hashUsers.TryGetValue(token, out var user);
			return Task.FromResult((result, user));
		}
	}
}