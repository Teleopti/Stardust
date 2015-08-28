namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IFindTenantForRta
	{
		bool Find(string rtaKey);
	}
}