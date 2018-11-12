using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IStateReader
	{
		IApplicationData ApplicationScopeData { get; }
	}
}