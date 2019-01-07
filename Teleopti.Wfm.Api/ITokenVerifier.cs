using System;
using System.Threading.Tasks;

namespace Teleopti.Wfm.Api
{
	public interface ITokenVerifier
	{
		Task<ValueTuple<bool, UserIdWithTenant>> TryGetUserAsync(string token);
	}
}