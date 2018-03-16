using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistExternalApplicationAccess
	{
		void Persist(ExternalApplicationAccess externalApplicationAccess);
		void Remove(int id, Guid personId);
	}
}