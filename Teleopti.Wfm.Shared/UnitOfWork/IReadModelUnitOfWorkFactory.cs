using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IReadModelUnitOfWorkFactory : IDisposable
	{
		void Configure();
		void StartUnitOfWork();
		void EndUnitOfWork(Exception exception);
	}
}