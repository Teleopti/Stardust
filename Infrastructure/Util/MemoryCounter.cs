using System;
using System.Globalization;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class MemoryCounter
	{
		private long maxMem = 0;

		public string CurrentMemoryConsumption()
		{
			var mem = GC.GetTotalMemory(true);
			if (mem > maxMem)
				maxMem = mem;
			return string.Format(CultureInfo.CurrentCulture, "Mem: {0:#.00} MB (max mem: {1:#} MB)", (double)mem / 1024 / 1024, maxMem / 1024 / 1024);
		}
	}
}
