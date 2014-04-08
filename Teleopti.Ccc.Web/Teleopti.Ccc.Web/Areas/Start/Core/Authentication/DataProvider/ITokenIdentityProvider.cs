namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public interface ITokenIdentityProvider
	{
		TokenIdentity RetrieveToken();
	}
}