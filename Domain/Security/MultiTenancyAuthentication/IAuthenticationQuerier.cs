using System;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQueryResult TryLogon(string userName, string password);
	}

	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		public AuthenticationQueryResult TryLogon(string userName, string password)
		{
			// just a fake for now, fejkar att vi fått datasource och personid från web service
			return new AuthenticationQueryResult { PersonId = new Guid("10957AD5-5489-48E0-959A-9B5E015B2B5C") 
				, Success = true, Tennant = "Teleopti WFM" };
		}
	}
}