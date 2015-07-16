using System;
using System.Globalization;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class MemoryCounter
	{
		private static MemoryCounter defaultInstance;
		private static readonly object defaultInstanceLocker = new object();
		private long _maxMem = 0;

		public static MemoryCounter DefaultInstance()
		{
			lock (defaultInstanceLocker)
			{
				return defaultInstance ?? (defaultInstance = new MemoryCounter());
			}
		}

		public double MaximumMemoryConsumption
		{
			get { return (double) _maxMem/1024/1024; } 
		}

		public double CurrentMemoryConsumption()
		{
			var mem = GC.GetTotalMemory(true);
			if (mem > _maxMem)
				_maxMem = mem;
			return (double) mem/1024/1024;
		}

		public string CurrentMemoryConsumptionString()
		{
			return string.Format(CultureInfo.CurrentCulture, "Mem: {0:#.00} MB (max mem: {1:#} MB)", CurrentMemoryConsumption(), MaximumMemoryConsumption);
		}
	}
}
