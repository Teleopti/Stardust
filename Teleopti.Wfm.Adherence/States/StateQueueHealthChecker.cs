using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IStateQueueHealthChecker
	{
		void Check();
		Health Health();
		void Reset();
	}

	public class Health
	{
		public bool Healthy;
		public int QueueSize;
	}

	public class AlwaysHealthyChecker : IStateQueueHealthChecker
	{
		public void Check()
		{
		}

		public Health Health()
		{
			return new Health {Healthy = true};
		}

		public void Reset()
		{
		}
	}

	public class StateQueueHealthChecker : IStateQueueHealthChecker
	{
		private readonly IStateQueueReader _reader;
		private readonly int _maxSize;
		private readonly PerTenant<Health> _health;

		public StateQueueHealthChecker(IStateQueueReader reader, IConfigReader config, ICurrentDataSource tenant)
		{
			_reader = reader;
			_maxSize = config.ReadValue("RtaStateQueueMaxSize", 100);
			_health = new PerTenant<Health>(tenant);
		}

		[AnalyticsUnitOfWork]
		public virtual void Check()
		{
			var count = _reader.Count();
			var healthy = count < _maxSize;
			_health.Set(new Health
			{
				Healthy = healthy,
				QueueSize = count
			});
		}

		public Health Health()
		{
			if (_health.Value == null)
				Check();
			return _health.Value;
		}

		public void Reset()
		{
			_health.Set(null);
		}
	}
}