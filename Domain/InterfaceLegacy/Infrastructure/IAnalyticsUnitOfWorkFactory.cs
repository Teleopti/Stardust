using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAnalyticsUnitOfWorkFactory : IDisposable
	{
		string ConnectionString { get; }
		IUnitOfWork CreateAndOpenUnitOfWork();
		IUnitOfWork CurrentUnitOfWork();
		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();
	}
}