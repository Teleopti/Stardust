using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IApplicationData : IDisposable
	{
		IDictionary<string, string> AppSettings { get; }
		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }
	}
	
}