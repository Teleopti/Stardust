namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class User :  IUser
	{
		public User(string name, string password)
		{
			Name = name;
			Password = password;
		}

		public string Name { get; private set; }
		public string Password { get; private set; }
	}
}