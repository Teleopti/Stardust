using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class UserValidator : IUserValidator
	{
		private StringBuilder errorMsgBuilder;
		private IList<IApplicationRole> validRoles;
		private const int MaxNameLength = 25;
		private const int MaxApplicationUserIdLength = 50;

		public UserValidator()
		{
			errorMsgBuilder = new StringBuilder();
			validRoles = new List<IApplicationRole>();
		}

		public bool Validate(RawUser user, IDictionary<string, IApplicationRole> availableRoles)
		{
			errorMsgBuilder.Clear();
			validRoles.Clear();
			var isUserValid = true;

			if (string.IsNullOrEmpty(user.ApplicationUserId) && string.IsNullOrEmpty(user.WindowsUser))
			{
				errorMsgBuilder.Append(Resources.NoLogonAccountErrorMsgSemicolon + " ");
				isUserValid = false;
			}
			else if (string.IsNullOrEmpty(user.ApplicationUserId) && !string.IsNullOrEmpty(user.Password))
			{
				errorMsgBuilder.Append(Resources.NoApplicationLogonAccountErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (!string.IsNullOrEmpty(user.ApplicationUserId) &&  string.IsNullOrEmpty(user.Password))
			{
				errorMsgBuilder.Append(Resources.EmptyPasswordErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (string.IsNullOrEmpty(user.Firstname) && string.IsNullOrEmpty(user.Lastname))
			{
				errorMsgBuilder.Append(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (user.Firstname.Length > MaxNameLength)
			{
				errorMsgBuilder.Append(Resources.TooLongFirstnameErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (user.Lastname.Length > MaxNameLength)
			{
				errorMsgBuilder.Append(Resources.TooLongLastnameErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (user.ApplicationUserId.Length > MaxApplicationUserIdLength)
			{
				errorMsgBuilder.Append(Resources.TooLongApplicationUserIdErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (!string.IsNullOrEmpty(user.Role))
			{
				var roles = parse(user.Role).ToList();
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
				else if (isUserValid)
				{
					roles.ForEach(r => validRoles.Add(availableRoles[r.ToUpper()]));
				}
			}
			return isUserValid;
		}
		private IEnumerable<string> parse(string roles)
		{

			const string splitPattern = "[^,\"]+|\"[^\"]*\"";
			var matches = Regex.Matches(roles, splitPattern);
			var result =
				(from object match in matches select match.ToString().Replace("\"", "").Trim())
				.Where(x => !string.IsNullOrEmpty(x));

			return new HashSet<string>(result);
		}
		public string ErrorMessage
		{
			get { return errorMsgBuilder.ToString(); }
		}

		public IList<IApplicationRole> ValidRoles
		{
			get { return validRoles; }
		} 
	}
}