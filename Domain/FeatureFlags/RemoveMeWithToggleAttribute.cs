using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	[AttributeUsage(AttributeTargets.All | AttributeTargets.Method, AllowMultiple = true)]
	public class RemoveMeWithToggleAttribute : Attribute
	{
		public RemoveMeWithToggleAttribute(params Toggles[] toggles)
		{
			TrickCompilerToThinkThisIsUsed = toggles;
		}

		public IEnumerable<Toggles> TrickCompilerToThinkThisIsUsed { get; private set; }
	}
}