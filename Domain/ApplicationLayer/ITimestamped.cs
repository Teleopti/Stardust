using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ITimestamped
	{
		DateTime Timestamp { get; set; }
	}
}