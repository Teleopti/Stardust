namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IFindTenantForRta
	{
		string Find(string rtaKey);
	}
}