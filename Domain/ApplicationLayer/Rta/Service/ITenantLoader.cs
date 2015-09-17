namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface ITenantLoader
	{
		string TenantNameByKey(string rtaKey);
		bool AuthenticateKey(string rtaKey);
	}
}