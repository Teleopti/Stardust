using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class UserValidator : IUserValidator
	{
		private const int maxNameLength = 25;
		private const int maxWindowUserLength = 100;
		private const int maxApplicationUserIdLength = 50;

		public bool Validate(RawUser user, IDictionary<string, IApplicationRole> availableRoles, StringBuilder errorMsgBuilder)
		{
			var isUserValid = true;

			if (string.IsNullOrEmpty(user.ApplicationUserId))
			{
				if (string.IsNullOrEmpty(user.WindowsUser))
				{
					errorMsgBuilder.Append(Resources.NoLogonAccountErrorMsgSemicolon + " ");
					isUserValid = false;
				}
				else if (!string.IsNullOrEmpty(user.Password))
				{
					errorMsgBuilder.Append(Resources.NoApplicationLogonAccountErrorMsgSemicolon + " ");
					isUserValid = false;
				}
			}
			else
			{
				if (user.ApplicationUserId.Length > maxApplicationUserIdLength)
				{
					errorMsgBuilder.Append(Resources.TooLongApplicationUserIdErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (string.IsNullOrEmpty(user.Password))
				{
					errorMsgBuilder.Append(Resources.EmptyPasswordErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (!(new EmailAddressAttribute().IsValid(user.ApplicationUserId)))
				{
					errorMsgBuilder.Append(Resources.ApplicationUserIdShouldBeAValidEmailAddressErrorMsg + " ");
					isUserValid = false;
				}
			}

			if (!string.IsNullOrEmpty(user.WindowsUser) && user.WindowsUser.Length > maxWindowUserLength)
			{
				errorMsgBuilder.Append(Resources.TooLongWindowsUserErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (string.IsNullOrEmpty(user.Firstname) && string.IsNullOrEmpty(user.Lastname))
			{
				errorMsgBuilder.Append(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (user.Firstname.Length > maxNameLength)
			{
				errorMsgBuilder.Append(Resources.TooLongFirstnameErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (user.Lastname.Length > maxNameLength)
			{
				errorMsgBuilder.Append(Resources.TooLongLastnameErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (!string.IsNullOrEmpty(user.Role))
			{
				var roles = StringHelper.SplitStringList(user.Role).ToList();
				var invalidRolesBuilder = new StringBuilder();
				var hasInvalidRole = false;

				foreach (var role in roles.Where(role => !availableRoles.ContainsKey(role.ToUpper())))
				{
					hasInvalidRole = true;
					var roleNotExist = (role.Contains(",") ? string.Format("\"{0}\"", role) : role) + ", ";
					invalidRolesBuilder.Append(roleNotExist);
				}

				if (hasInvalidRole)
				{
					var errorRolesMsg = invalidRolesBuilder.ToString();
					errorRolesMsg = errorRolesMsg.Substring(0, errorRolesMsg.Length - 2);
					errorRolesMsg = string.Format(Resources.RoleXNotExistErrorMsgSemicolon + " ", errorRolesMsg);
					errorMsgBuilder.Append(errorRolesMsg);
					isUserValid = false;
				}
			}

			return isUserValid;
		}
	}
}