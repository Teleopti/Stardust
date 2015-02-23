using System.Collections.Generic;
using System.Globalization;
using log4net;
using System.Linq;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	public class Synchronizer
	{
		private readonly IUserHandler _userHandler;
		private readonly Dictionary<string, User> _analyzerUsersDictionary;
		private readonly Dictionary<string, UserDto> _clientUsersDictionary;
		readonly ILog _logger = LogManager.GetLogger(typeof(Synchronizer));
		static private bool isWindowsInstallation { get { return PermissionInformation.IsPmAuthenticationWindows; }}

		private Synchronizer() { }

		public Synchronizer(IEnumerable<UserDto> users, IUserHandler userHandler)
			: this()
		{
			_userHandler = userHandler;

			_logger.Debug("Getting Analyzer users.");
			User[] analyzerUsers = _userHandler.GetAnalyzerUsers();

			_analyzerUsersDictionary = new Dictionary<string, User>();
			_clientUsersDictionary = new Dictionary<string, UserDto>();

			if (users != null)
			{
				foreach (UserDto userDto in users)
				{
					_clientUsersDictionary.Add(userDto.UserName.ToUpperInvariant(), userDto);
				}
			}

			if (!isWindowsInstallation) //If Anonymous mode fake the Analyzer users
			{
				_analyzerUsersDictionary = _clientUsersDictionary.ToDictionary(k => k.Key, v => new User { Name = v.Value.UserName });
			}
			else
			{
				if (analyzerUsers != null)
				{
					foreach (User analyzerUser in analyzerUsers)
					{
						_analyzerUsersDictionary.Add(analyzerUser.Name.Trim().ToUpperInvariant(), analyzerUser);
					}
				}

				_logger.DebugFormat("Client users to synchronize: {0}", _clientUsersDictionary.Count);
				_logger.DebugFormat("Existing Analyzer users: {0}", _analyzerUsersDictionary.Count);
			}
		}

		public ResultDto SynchronizeUsers()
		{
			var resultDto = new ResultDto();
			var syncFailureCount = 0;

			//Before sync of users we make sure the default roles have correct permissions
			_userHandler.SetDefaultRolePermission();

			foreach (UserDto userDto in _clientUsersDictionary.Values)
			{
				bool userExists = _analyzerUsersDictionary.ContainsKey(userDto.UserName.ToUpperInvariant());
				bool userUpdated;

				if (userExists)
				{
					User analyzerUser = _analyzerUsersDictionary[userDto.UserName.ToUpperInvariant()];
					// Update users role membership
					if (isWindowsInstallation)
					{
						userUpdated = _userHandler.UpdateRoleMembership(userDto, analyzerUser);
					}
					else
						userUpdated = true;

					if (userUpdated)
					{
						resultDto.AddUsersUpdated(userDto);
						resultDto.AddValidAnalyzerUser(userDto);
					}
					else
						syncFailureCount++;
				}
				else
				{
					// Need to create the user
					if (isWindowsInstallation)
					{
						_userHandler.Add(userDto);
					}
					resultDto.AddUsersInserted(userDto);
					resultDto.AddValidAnalyzerUser(userDto);
				}
			}

			// Remove users that exist in Analyzer but not in clientlist - Do not remove Analyzer Admin user!!!
			foreach (User user in _analyzerUsersDictionary.Values)
			{
				if (!_clientUsersDictionary.ContainsKey(user.Name.Trim().ToUpperInvariant()))
				{
					// Delete user from analyzer, do not remove analyzer admin
					if (isWindowsInstallation && _userHandler.Delete(user))
					{
						var userDto = new UserDto { UserName = user.Name.Trim() };
						resultDto.AddUsersDeleted(userDto);
					}
				}
			}

			_logger.DebugFormat("Users inserted: {0}", resultDto.UsersInserted.Count);
			_logger.DebugFormat("Users updated: {0}", resultDto.UsersUpdated.Count);
			_logger.DebugFormat("Users deleted: {0}", resultDto.UsersDeleted.Count);
			_logger.DebugFormat("Users valid in Analyzer: {0}", resultDto.ValidAnalyzerUsers.Count);
			_logger.DebugFormat("Users failed to update: {0}", syncFailureCount);

			CheckSynchronizationException(resultDto.UsersUpdated.Count + resultDto.UsersInserted.Count, syncFailureCount);

			_logger.Debug("Synchronization of users succeeded.");
			resultDto.Success = true;
			resultDto.AffectedUsersCount = resultDto.UsersInserted.Count + resultDto.UsersUpdated.Count + resultDto.UsersDeleted.Count;

			return resultDto;
		}

		private static void CheckSynchronizationException(int userCount, int syncFailureCount)
		{
			if (userCount == 0 && syncFailureCount > 0)
			{
				var message = string.Format(CultureInfo.InvariantCulture, "Failure trying to update {0} user(s). Probably caused by users no longer exist in Active Directory.", syncFailureCount);
				throw new PmSynchronizeException(message);
			}
		}
	}
}