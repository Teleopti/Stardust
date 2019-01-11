namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ILoggedOnUser
	{
		IPerson CurrentUser();
		string CurrentUserName();
	}

	public interface ILoggedOnUserIsPerson
	{
		bool IsPerson(IPerson person);
	}
}