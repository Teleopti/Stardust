using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IWriteSideRepository<T> : IWriteSideRepositoryTypedId<T, Guid>
	{
	}
}