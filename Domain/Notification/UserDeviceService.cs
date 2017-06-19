using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Notification
{
	public class UserDeviceService
	{
		private readonly IUserDeviceRepository _userDeviceRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public UserDeviceService(IUserDeviceRepository userDeviceRepository, ILoggedOnUser loggedOnUser)
		{
			_userDeviceRepository = userDeviceRepository;
			_loggedOnUser = loggedOnUser;
		}

		public void StoreUserDevice(string token)
		{
			var currentDevice = _userDeviceRepository.FindByToken(token);
			if (currentDevice != null)
			{
				_userDeviceRepository.Remove(currentDevice);
			}

			_userDeviceRepository.Add(new UserDevice
			{
				Token = token,
				Owner = _loggedOnUser.CurrentUser()
			});
		}

		public IList<string> GetUserTokens()
		{
			return _userDeviceRepository.Find(_loggedOnUser.CurrentUser()).Select(d => d.Token).ToList();
		}
	}
}