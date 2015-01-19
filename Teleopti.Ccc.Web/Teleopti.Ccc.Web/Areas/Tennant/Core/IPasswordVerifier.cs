namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IPasswordVerifier
	{
		bool Check(string userPassword, string existingPasswordInDb);
	}
}