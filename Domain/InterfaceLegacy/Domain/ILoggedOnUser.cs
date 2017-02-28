namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ILoggedOnUser
	{
		IPerson CurrentUser();
	}

	public interface ILoggedOnUserIsPerson
	{
		bool IsPerson(IPerson person);
	}
}