using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IAnalyticsUnitOfWorkFactory : IDisposable
	{
		string ConnectionString { get; }
		IUnitOfWork CreateAndOpenUnitOfWork();
		IUnitOfWork CurrentUnitOfWork();
		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();
	}
}