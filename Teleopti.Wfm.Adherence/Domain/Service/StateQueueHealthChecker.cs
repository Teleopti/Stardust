using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IStateQueueHealthChecker
	{
		void Check();
		bool Healthy();
		void Reset();
	}

	public class AlwaysHealthyChecker : IStateQueueHealthChecker
	{
		public void Check()
		{
		}

		public bool Healthy()
		{
			return true;
		}

		public void Reset()
		{
		}
	}

	public class StateQueueHealthChecker : IStateQueueHealthChecker
	{
		private readonly IStateQueueReader _reader;
		private readonly int _maxSize;
		private readonly PerTenant<bool?> _healthy;

		public StateQueueHealthChecker(IStateQueueReader reader, IConfigReader config, ICurrentDataSource tenant)
		{
			_reader = reader;
			_maxSize = config.ReadValue("RtaStateQueueMaxSize", 100);
			_healthy = new PerTenant<bool?>(tenant);
		}

		[AnalyticsUnitOfWork]
		public virtual void Check()
		{
			_healthy.Set(_reader.Count() < _maxSize);
		}

		public bool Healthy()
		{
			if (!_healthy.Value.HasValue)
				Check();
			return _healthy.Value.GetValueOrDefault();
		}

		public void Reset()
		{
			_healthy.Set(null);
		}
	}
}