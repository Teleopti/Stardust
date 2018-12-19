using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
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