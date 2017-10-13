using System.Collections.Generic;

namespace CheckPreRequisites.Checks
{
	public abstract class Limits
	{
		public IList<Limit> All { get; set; }
	}

	public class DbLimits : Limits
	{
		public DbLimits()
		{
			All = new List<Limit>
			{
				new Limit(400, 4, 8),
				new Limit(1500, 4, 16),
				new Limit(4000, 6, 32),
				new Limit(10000, 8, 64),
				new Limit(20000, 16, 128),
				new Limit(int.MaxValue, 28, 256)
			};
		}
	}

	public class WebLimits : Limits
	{
		public WebLimits()
		{
			All = new List<Limit>
			{
				new Limit(400, 4, 8),
				new Limit(1500, 4, 16),
				new Limit(4000, 8, 32),
				new Limit(10000, 16, 64),
				new Limit(20000, 8, 16, 4),
				new Limit(int.MaxValue, 8, 16, 6)
			};
		}
	}

	public class WorkerLimits : Limits
	{
		public WorkerLimits()
		{
			All = new List<Limit>
			{
				new Limit(400, 4, 8),
				new Limit(1500, 4, 16),
				new Limit(4000, 6, 16),
				new Limit(10000, 8, 16),
				new Limit(20000, 8, 32, 4),
				new Limit(int.MaxValue, 8, 32, 6)
			};
		}
	}

	public class Limit
	{
		public Limit(int maxAgents, int processor, int memory, int numberOfServers = 1)
		{
			MaxAgents = maxAgents;
			Processor = processor;
			Memory = memory;
			NumberOfServers = numberOfServers;
		}

		public int MaxAgents;
		public int Memory;
		public int NumberOfServers;
		public int Processor;
	}
}