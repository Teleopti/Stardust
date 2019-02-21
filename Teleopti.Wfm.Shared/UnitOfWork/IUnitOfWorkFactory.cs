using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IUnitOfWorkFactory : IDisposable
	{
		string Name { get; }
		string ConnectionString { get; }
		IUnitOfWork CreateAndOpenUnitOfWork();
		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();
		IUnitOfWork CurrentUnitOfWork();
		bool HasCurrentUnitOfWork();
	}
}