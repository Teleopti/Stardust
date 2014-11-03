using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IReadModelUnitOfWorkFactory
	{
		void Configure(string connectionString);
		void StartUnitOfWork();
		void EndUnitOfWork(Exception exception);
	}
}