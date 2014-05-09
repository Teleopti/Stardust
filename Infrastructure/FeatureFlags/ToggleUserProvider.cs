using Teleopti.Interfaces.Domain;
using Toggle.Net.Configuration;

namespace Teleopti.Ccc.Infrastructure.FeatureFlags
{
	public class ToggleUserProvider : IUserProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ToggleUserProvider(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public string CurrentUser()
		{
			var currentUser = _loggedOnUser.CurrentUser();
			return currentUser == null ? null : currentUser.Id.Value.ToString();
		}
	}
}