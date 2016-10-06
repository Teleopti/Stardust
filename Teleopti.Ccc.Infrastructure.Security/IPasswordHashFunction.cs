namespace Teleopti.Ccc.Infrastructure.Security
{
	public interface IPasswordHashFunction
	{
		string CreateHash(string password);
		bool Verify(string password, string hash);
		bool IsGeneratedByThisFunction(string hash);
	}
}