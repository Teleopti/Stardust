using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class UserValidator : IUserValidator
	{
		private readonly IApplicationRoleRepository _roleRepository;
		private StringBuilder errorMsgBuilder;
		private IList<IApplicationRole> validRoles;
		private const int MaxNameLength = 25;
		private const int MaxApplicationUserIdLength = 50;

		public UserValidator(IApplicationRoleRepository roleRepository)
		{
			_roleRepository = roleRepository;
			errorMsgBuilder = new StringBuilder();
			validRoles = new List<IApplicationRole>();
		}

		public bool Validate(RawUser user)
		{
			errorMsgBuilder.Clear();
			validRoles.Clear();

			var availableRoles = new Dictionary<string, IApplicationRole>();
			var allRoles = _roleRepository.LoadAllApplicationRolesSortedByName();
			var isUserValid = true;
			allRoles.ForEach(r =>
			{
				if (!availableRoles.ContainsKey(r.DescriptionText.ToUpper()))
				{
					availableRoles.Add(r.DescriptionText.ToUpper(), r);
				}
			});
			if (string.IsNullOrEmpty(user.ApplicationUserId) && string.IsNullOrEmpty(user.WindowsUser))
			{
				errorMsgBuilder.Append(Resources.NoLogonAccountErrorMsgSemicolon + " ");
				isUserValid = false;
			}
			else if (string.IsNullOrEmpty(user.ApplicationUserId))
			{
				errorMsgBuilder.Append(Resources.NoApplicationLogonAccountErrorMsgSemicolon + " ");
				isUserValid = false;
			}

			if (string.IsNullOrEmpty(user.Password))
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
				var roles = user.Role.Split(',').Select(x => x.Trim()).ToList();
				var invalidRolesBuilder = new StringBuilder();
				var hasInvalidRole = false;

				foreach (var role in roles.Where(role => !availableRoles.ContainsKey(role.ToUpper())))
				{
					hasInvalidRole = true;
					var roleNotExist = role + ", ";
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