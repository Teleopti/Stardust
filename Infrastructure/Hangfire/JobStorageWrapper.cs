using Hangfire;
using Hangfire.Storage;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface IJobStorageWrapper
	{
		JobStorage GetJobStorage();
	}

	public class JobStorageWrapper : IJobStorageWrapper
	{
		public JobStorage GetJobStorage()
		{
			return JobStorage.Current;
		}
	}

	public class FakeJobStorageWrapper : IJobStorageWrapper
	{
		public JobStorage GetJobStorage()
		{
			return new FakeJobStorage();
		}

		private class FakeJobStorage : JobStorage
		{
			public override IMonitoringApi GetMonitoringApi()
			{
				return null;
			}

			public override IStorageConnection GetConnection()
			{
				return null;
			}
		}
	}
}