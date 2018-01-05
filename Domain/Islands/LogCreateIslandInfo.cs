using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Islands
{
	public class LogCreateIslandInfo
	{
		public LogCreateIslandInfo(IEnumerable<Island> islands, TimeSpan timeToGenerate)
		{
			Islands = islands;
			TimeToGenerate = timeToGenerate;
		}
		public IEnumerable<Island> Islands { get; }
		public TimeSpan TimeToGenerate { get; }
	}
}