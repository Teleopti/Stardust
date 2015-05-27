using System;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobMultipleDateItem
	{

		DateTime EndDateLocal { get; }

		DateTime StartDateLocal { get; }

		DateTime StartDateUtc { get; }

		DateTime EndDateUtc { get; }

		DateTime StartDateUtcFloor { get; }

		DateTime EndDateUtcCeiling { get; }
	}
}
