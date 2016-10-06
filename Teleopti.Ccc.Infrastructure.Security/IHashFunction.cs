namespace Teleopti.Ccc.Infrastructure.Security
{
	public interface IHashFunction
	{
		string CreateHash(string password);
		bool Verify(string password, string hash);
		bool IsGeneratedByThisFunction(string hash);
	}
}