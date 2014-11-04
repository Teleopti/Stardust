using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IReadModelUnitOfWorkFactory
	{
		void Configure();
		void StartUnitOfWork();
		void EndUnitOfWork(Exception exception);
	}
}