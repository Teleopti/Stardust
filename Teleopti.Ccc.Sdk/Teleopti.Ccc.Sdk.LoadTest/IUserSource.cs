namespace Teleopti.Ccc.Sdk.LoadTest
{
	public interface IUserSource
	{
		IUser GetUser(int testNumber);
	}

	public interface IUser
	{
		string Name { get; }
		string Password { get; }
	}
}