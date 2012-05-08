using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class Now : INow
	{
		public DateTime Time
		{
			get { return DateTime.Now; }
		}

		public DateTime UtcTime
		{
			get { return DateTime.UtcNow; }
		}
	}
}