namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiPrincipalWithUnsafePerson
	{
		object UnsafePersonObject();
		void ChangePrincipal(ITeleoptiPrincipalWithUnsafePerson principal);
	}
}