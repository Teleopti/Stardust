using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
	public class SdkCurrentTenantCredentials : ICurrentTenantCredentials
	{
		private readonly ICurrentPersonContainer _currentPersonContainer;

		public SdkCurrentTenantCredentials(ICurrentPersonContainer currentPersonContainer)
		{
			_currentPersonContainer = currentPersonContainer;
		}

		public TenantCredentials TenantCredentials()
		{
			var personContainer = _currentPersonContainer.Current();
			return new TenantCredentials(personContainer.Person.Id.Value, personContainer.TenantPassword);
		}
	}
}