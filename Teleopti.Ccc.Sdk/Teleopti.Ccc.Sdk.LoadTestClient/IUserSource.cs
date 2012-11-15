namespace Teleopti.Ccc.Sdk.LoadTestClient
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