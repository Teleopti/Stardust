using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IApplicationData
	{
		IDictionary<string, string> AppSettings { get; }
		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }
	}
	
}