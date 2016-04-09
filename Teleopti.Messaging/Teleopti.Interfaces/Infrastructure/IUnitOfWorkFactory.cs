using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAnalyticsUnitOfWorkFactory : IDisposable
	{
		string ConnectionString { get; }
		IUnitOfWork CreateAndOpenUnitOfWork();
		IUnitOfWork CurrentUnitOfWork();
		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();
	}

	public interface IUnitOfWorkFactory : IDisposable
	{
		string Name { get; }
		long? NumberOfLiveUnitOfWorks { get; }
		IAuditSetter AuditSetting { get; }
		string ConnectionString { get; }

		IUnitOfWork CreateAndOpenUnitOfWork();
		IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter);

		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();

		IUnitOfWork CurrentUnitOfWork();
		bool HasCurrentUnitOfWork();
	}
}