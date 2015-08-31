namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaTenantAuthenticator : IRtaAuthenticator
	{
		private readonly IFindTenantForRta _findTenantForRta;

		public RtaTenantAuthenticator(IFindTenantForRta findTenantForRta)
		{
			_findTenantForRta = findTenantForRta;
		}

		public string Autenticate(string authenticationKey)
		{
			return _findTenantForRta.Find(authenticationKey);
		}
	}
}