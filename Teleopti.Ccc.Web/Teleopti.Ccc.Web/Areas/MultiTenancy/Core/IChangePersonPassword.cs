namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IChangePersonPassword
	{
		void Modify(string userName, string oldPassword, string newPassword);
	}
}