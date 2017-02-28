using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IUnitOfWorkFactory : IDisposable
	{
		string Name { get; }
		IAuditSetter AuditSetting { get; }
		string ConnectionString { get; }

		IUnitOfWork CreateAndOpenUnitOfWork();
		IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter);

		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();

		IUnitOfWork CurrentUnitOfWork();
		bool HasCurrentUnitOfWork();
	}
}