using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IReadModelUnitOfWorkFactory : IDisposable
	{
		void Configure();
		void StartUnitOfWork();
		void EndUnitOfWork(Exception exception);
	}
}