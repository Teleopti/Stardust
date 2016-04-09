using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		public string Name { get; set; }
		public long? NumberOfLiveUnitOfWorks { get; set; }
		public IAuditSetter AuditSetting { get; set; }
		public string ConnectionString { get; set; }
		
		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork();
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return new FakeUnitOfWork();
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork();
		}

		private bool _hasCurrentUnitOfWork;
		public bool HasCurrentUnitOfWork()
		{
			return _hasCurrentUnitOfWork;
		}

		public void SetHasCurrentUnitOfWork()
		{
			_hasCurrentUnitOfWork = true;
		}

		public void Dispose()
		{
		}

	}
}