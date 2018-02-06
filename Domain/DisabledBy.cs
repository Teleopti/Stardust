using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class DisabledBy : Attribute
	{
		public DisabledBy(params Toggles[] toggles)
		{
			Toggles = toggles;
		}

		public IEnumerable<Toggles> Toggles { get; private set; }
	}

}