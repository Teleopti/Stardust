using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	public class UserHandler : IUserHandler
	{
		private readonly IClientProxy _analyzerProxy;
		private Role _roleAdministrator;
		private Role _roleReportDesigner;
		private Role _roleGeneralUser;
		private User _adminUser;
		Dictionary<int, Dictionary<int, Role>> _userRoleDictionary;
		private User[] _analyzerUsers;
		readonly ILog _logger = LogManager.GetLogger(typeof(UserHandler));

		private UserHandler() { }

		public UserHandler(IClientProxy analyzerProxy)
			: this()
		{
			_analyzerProxy = analyzerProxy;
			SetAnalyzerRoles();
			SetUserRoles();
		}

		private void SetUserRoles()
		{
			_userRoleDictionary = new Dictionary<int, Dictionary<int, Role>>();
			
			foreach (User analyzerUser in AnalyzerUsers)
			{
				Role[] userRoles = _analyzerProxy.GetUserRoles(analyzerUser.Name.Trim());
				var roleDictionary = new Dictionary<int, Role>();

				foreach (Role userRole in userRoles)
				{
					SetAdminUser(analyzerUser, userRole);
					roleDictionary.Add(userRole.Id, userRole);
				}

				if (roleDictionary.Count > 0)
				{
					_userRoleDictionary.Add(analyzerUser.Id, roleDictionary);
				}
			}
		}

		private void SetAdminUser(User user, Role role)
		{
			if (_adminUser == null)
			{
				if (role.Id == _roleAdministrator.Id)
				{
					// This user is the one and only Administrator of Analyzer!
					_adminUser = user;
				}
			}
		}

		private void SetAnalyzerRoles()
		{
			// Get the three pre-defined Analyzer roles
			Role[] roles = _analyzerProxy.GetRoles();

			if (roles != null && roles.Length > 0)
			{
				foreach (Role userRole in roles)
				{
					switch (userRole.Name.Trim().ToUpperInvariant())
					{
						//role name have changed over version. We cover both
						//_roleAdministrator
						case "ADMINISTRATOR":
						case "ADMINISTRATORS":
							_roleAdministrator = userRole;
							break;
						//_roleReportDesigner
						case "REPORT DESIGNER":
						case "REPORT DESIGNERS":
							_roleReportDesigner = userRole;
							break;
						//_roleGeneralUser
						case "GENERAL USER":
						case "GENERAL USERS":
							_roleGeneralUser = userRole;
							break;
						//_roleMobileUser
						case "MOBILE USERS":
							_logger.DebugFormat(CultureInfo.InvariantCulture, "Analyzer role '{0}' is not in use by Teleopti PM.", userRole.Name.Trim());
							break;
					}
				}
			}
		}

		private Role GetRoleToAssign(UserDto userDto)
		{
			if (userDto.AccessLevel == 1)
			{
				return _roleGeneralUser;
			}

			if (userDto.AccessLevel == 2)
			{
				return _roleReportDesigner;
			}

			return null;
		}

		private static PermissionLevel GetPermissionLevel(UserDto userDto)
		{
			switch (userDto.AccessLevel)
			{
				case 1:
					return PermissionLevel.GeneralUser;
				case 2:
					return PermissionLevel.ReportDesigner;
				default:
					return PermissionLevel.None;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Add(UserDto user)
		{
			Role roleToAssign = GetRoleToAssign(user);
			if (roleToAssign != null)
			{
				// User need a role to be member of, else user is not created.

				_logger.DebugFormat("Adding new user {0}.", user.UserName);
				User newUser = _analyzerProxy.AddUser(user.UserName);
				if (newUser.Success)
				{
					// User created in Analyzer, now assign role membership
					AssignRoleMembership(newUser, roleToAssign);
				}
				else
				{
					string msg = string.Format(CultureInfo.InvariantCulture, "Failed to add user '{0}' to Analyzer.", user.UserName);
					_logger.Debug(msg);
					throw new PmSynchronizeException(msg);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Delete(User user)
		{
			// Do not remove Analyzer Admin user!!!
			if (user.Id != _adminUser.Id)
			{
				_logger.DebugFormat("Deleting user '{0}'.", user.Name.Trim());
				if (_analyzerProxy.DeleteUser(user))
				{
					return true;
				}

				string msg = string.Format(CultureInfo.InvariantCulture, "Failed to delete user '{0}' from Analyzer.", user.Name.Trim());
				_logger.Debug(msg);
				throw new PmSynchronizeException(msg);
			}
			return false;
		}

		private void AssignRoleMembership(CatalogItem user, CatalogItem role)
		{
			_logger.DebugFormat("Updating existing user '{0}'", user.Name.Trim());
			if (!_analyzerProxy.AssignRoleMembership(user.Id, role.Id))
			{
				string msg = string.Format(CultureInfo.InvariantCulture,
										   "Failed to assign membership of role '{0}' for user '{1}' in Analyzer.", role.Name.Trim(), user.Name.Trim());
				_logger.Debug(msg);
				throw new PmSynchronizeException(msg);
			}
		}

		private void DeleteRoleMembership(CatalogItem user, CatalogItem role)
		{
			_logger.DebugFormat("Deleting role '{0}' for user '{1}'", role.Name.Trim(), user.Name.Trim());
			if (!_analyzerProxy.DeleteUserRoleMembership(user.Id, role.Id))
			{
				string msg = string.Format(CultureInfo.InvariantCulture,
										   "Failed to remove potential membership of role '{0}' for user '{1}' in Analyzer.",
										   role.Name.Trim(), user.Name.Trim());
				_logger.Debug(msg);
				throw new PmSynchronizeException(msg);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool UpdateRoleMembership(UserDto userDto, User user)
		{
			if (_userRoleDictionary.ContainsKey(user.Id))
			{
				// User is already member of one role or more

				Dictionary<int, Role> roleDictionary = _userRoleDictionary[user.Id];

				if (roleDictionary.ContainsKey(0))
				{
					_logger.WarnFormat(CultureInfo.InvariantCulture, "Failed to update role membership for user '{0}'. Probably because user no longer exist in the Active Directory.", user.Name.Trim());
					RemoveAllRoleMembership(user);
					return false;
				}

				// Only, if user is NOT member of Administrator role, changes will be done.
				if (!roleDictionary.ContainsKey(_roleAdministrator.Id))
				{
					PermissionLevel permissionLevel = GetPermissionLevel(userDto);

					switch (permissionLevel)
					{
						case PermissionLevel.GeneralUser:
							if (!roleDictionary.ContainsKey(_roleGeneralUser.Id))
							{
								// Make user member of General User role
								AssignRoleMembership(user, _roleGeneralUser);
							}

							// Remove possible membership of Report Designer
							DeleteRoleMembership(user, _roleReportDesigner);
							break;
						case PermissionLevel.ReportDesigner:
							if (!roleDictionary.ContainsKey(_roleReportDesigner.Id))
							{
								// Make user member of Report Designer role
								AssignRoleMembership(user, _roleReportDesigner);
							}

							// Remove possible membership of General User
							DeleteRoleMembership(user, _roleGeneralUser);
							break;
						default:
							// Should not be member of any role. Remove possible role memberships
							RemoveAllRoleMembership(user);
							break;
					}
				}
			}
			else
			{
				// User does not belong to role - assign correct role membership
				Role roleToAssign = GetRoleToAssign(userDto);
				if (roleToAssign != null)
				{
					_logger.DebugFormat("Adding new user '{0}'", user.Name.Trim());
					AssignRoleMembership(user, roleToAssign);
				}
			}
			return true;
		}

		private void RemoveAllRoleMembership(User user)
		{
			DeleteRoleMembership(user, _roleGeneralUser);
			DeleteRoleMembership(user, _roleReportDesigner);
		}

		public void SetDefaultRolePermission()
		{
			_logger.Debug("Setting default permissions for the Analyzer roles.");
			if (!_analyzerProxy.SetDefaultRolePermission(_roleGeneralUser.Id, _roleReportDesigner.Id))
			{
				_logger.Debug("Failed setting default permissions for the Analyzer roles!");
				throw new PmSynchronizeException("Failed to set default permissions for the Analyzer roles.");
			}
		}

		private User[] AnalyzerUsers
		{
			get
			{
				if (_analyzerUsers == null)
				{
					User[] users = _analyzerProxy.GetUsers();

					if (users != null && users.Length > 0)
					{
						if (users.Any(user => !user.Success))
							throw new PmGetUsersException("Failed to get user from Analyzer.");
						_analyzerUsers = users;
					}
				}

				return _analyzerUsers;
			}
		}

		public User[] GetAnalyzerUsers()
		{
			return AnalyzerUsers;
		}
	}
}