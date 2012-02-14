using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	public interface IUserHandler
	{
		void Add(UserDto user);
		bool Delete(User user);
		bool UpdateRoleMembership(UserDto userDto, User user);
		void SetDefaultRolePermission();
		User[] GetAnalyzerUsers();
	}
}