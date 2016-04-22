using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class UseNotOnToggle : Attribute
	{
		public UseNotOnToggle(params Toggles[] toggles)
		{
			Toggles = toggles;
		}

		public IEnumerable<Toggles> Toggles { get; private set; }
	}

}