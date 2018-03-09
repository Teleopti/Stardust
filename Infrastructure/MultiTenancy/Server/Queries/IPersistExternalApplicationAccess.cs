namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistExternalApplicationAccess
	{
		void Persist(ExternalApplicationAccess externalApplicationAccess);
	}
}