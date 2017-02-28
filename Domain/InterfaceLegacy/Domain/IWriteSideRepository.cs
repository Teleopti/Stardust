using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IWriteSideRepository<T> : IWriteSideRepositoryTypedId<T, Guid>
	{
	}
}