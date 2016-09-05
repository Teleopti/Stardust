namespace Teleopti.Ccc.Domain.Logon
{
	public interface ITokenIdentityProvider
	{
		TokenIdentity RetrieveToken();
	}

	public class TokenIdentity
	{
		public string UserIdentifier { get; set; }
		public string OriginalToken { get; set; }
		public bool IsTeleoptiApplicationLogon { get; set; }
		public bool IsPersistent { get; set; }
	}
}