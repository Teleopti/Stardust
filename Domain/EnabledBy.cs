using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class EnabledBy : Attribute
	{
		public EnabledBy(params Toggles[] toggles)
		{
			Toggles = toggles;
		}

		public IEnumerable<Toggles> Toggles { get; private set; }
	}

}