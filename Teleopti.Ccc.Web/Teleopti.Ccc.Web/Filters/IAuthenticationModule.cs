namespace Teleopti.Ccc.Web.Filters
{
	public interface IAuthenticationModule
	{
		string Issuer { get; }
		string Realm { get; }
	}
}