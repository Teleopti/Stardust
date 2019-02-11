using System;
using System.Globalization;
using System.Runtime;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class MemoryCounter
	{
		private static MemoryCounter defaultInstance;
		private static readonly object defaultInstanceLocker = new object();
		private long _maxMem;

		public static MemoryCounter DefaultInstance()
		{
			lock (defaultInstanceLocker)
			{
				return defaultInstance ?? (defaultInstance = new MemoryCounter());
			}
		}

		public double MaximumMemoryConsumption => (double) _maxMem/1024/1024;

		public double CurrentMemoryConsumption()
		{
			GC.Collect();
			var mem = GC.GetTotalMemory(true);
			if (mem > _maxMem)
			{
				_maxMem = mem;
			}
			return (double) mem/1024/1024;
		}

		public string CurrentMemoryConsumptionString()
		{
			var result = GCSettings.IsServerGC ? "server" : "workstation";

			return string.Format(CultureInfo.CurrentCulture, "Mem: {0:#.00} MB (max mem: {1:#} MB)", CurrentMemoryConsumption(), MaximumMemoryConsumption) + " GC is " + result;
		}
	}
}
