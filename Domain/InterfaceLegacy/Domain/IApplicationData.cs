using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IApplicationData : IDisposable
	{
		IDictionary<string, string> AppSettings { get; }
		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }
	}
	
}